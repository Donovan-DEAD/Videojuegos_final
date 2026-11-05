using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Puebla cada cuarto con barreras (validadas para no sellarlo), props de decoración y enemigos del ZoneThemeSO,
    /// escalando densidad y nivel de enemigos con PlayerStats.Level mediante DifficultyProfileSO.
    /// </summary>
    public class RoomContentSpawner : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private Spawner spawner;
        [SerializeField] private float propMaxSize = 3f;
        [SerializeField] private int propEdgeMargin = 2;

        private WorldConfigSO config;
        private int cachedPlayerLevel = 1;
        private int cachedWorldIndex;
        private static Sprite whiteSprite;

        /// <summary>Cantidad de cuartos que puede poblar el generador actual.</summary>
        public int RoomCount => mapGenerator != null && mapGenerator.Rooms != null ? mapGenerator.Rooms.Count : 0;

        /// <summary>Inyecta la configuración y resuelve referencias a MapGenerator y Spawner si faltan.</summary>
        public void Configure(WorldConfigSO worldConfig)
        {
            config = worldConfig;

            if (mapGenerator == null)
            {
                mapGenerator = FindAnyObjectByType<MapGenerator>();
            }

            if (spawner == null)
            {
                spawner = FindAnyObjectByType<Spawner>();
            }
        }

        /// <summary>Cachea el nivel del jugador y el índice de mundo para poblar cuarto a cuarto.</summary>
        public void BeginSpawn(System.Random rng)
        {
            if (config == null || mapGenerator == null || mapGenerator.TargetTilemap == null)
            {
                return;
            }

            PlayerStats stats = FindAnyObjectByType<PlayerStats>();
            cachedPlayerLevel = stats != null ? stats.Level : 1;

            RunManager runManager = FindAnyObjectByType<RunManager>();
            cachedWorldIndex = runManager != null ? runManager.CurrentWorldIndex : 0;
        }

        /// <summary>Puebla el cuarto indicado usando los valores cacheados por BeginSpawn.</summary>
        public void SpawnRoomAt(int index, System.Random rng)
        {
            if (config == null || mapGenerator == null || index < 0 || index >= mapGenerator.Rooms.Count)
            {
                return;
            }

            SpawnRoom(index, mapGenerator.Rooms[index], rng, cachedPlayerLevel, cachedWorldIndex);
        }

        /// <summary>Recorre los cuartos del MapGenerator y los puebla según el nivel del jugador y el índice de mundo.</summary>
        public void SpawnAll(System.Random rng)
        {
            if (config == null || mapGenerator == null || mapGenerator.TargetTilemap == null)
            {
                return;
            }

            BeginSpawn(rng);

            for (int i = 0; i < mapGenerator.Rooms.Count; i++)
            {
                SpawnRoomAt(i, rng);
            }
        }

        private void SpawnRoom(int roomIndex, RoomPlacement room, System.Random rng, int playerLevel, int worldIndex)
        {
            if (room.role == CellRole.Special)
            {
                SpawnMiniBoss(room);
                SpawnMinibossTint(room);
                return;
            }
            
            
            if (room.role == CellRole.Merchant)
            {
                SpawnMerchant(room);
                return;
            }
        
            ZoneThemeSO theme = config.ThemeFor(room.tag);
            if (theme == null)
            {
                theme = config.ThemeFor(RoomTag.None);
            }
        
            if (theme == null)
            {
                return;
            }
        
            RectInt area = RoomArea(room);
            if (area.width <= 2 || area.height <= 2)
            {
                return;
            }
        
            bool[,] occupied = BuildOccupied(area);
            HashSet<Vector2Int> keepClear = KeepClear(roomIndex, area);
        
            bool combat = room.role != CellRole.Start;
            List<Vector2Int> barrierFootprints = new List<Vector2Int>();
        
            if (combat)
            {
                PlaceBarriers(theme, area, occupied, keepClear, barrierFootprints, rng);
            }
        
            PlaceProps(theme, area, occupied, keepClear, rng);
        
            if (combat)
            {
                SpawnEnemies(room, theme, area, occupied, rng, playerLevel, worldIndex);
            }
        }

        private RectInt RoomArea(RoomPlacement room)
        {
            int cellSize = config.ChunkSize;
            Vector3Int origin = mapGenerator.ChunkToCell(room.originCell);
            return new RectInt(origin.x, origin.y, room.sizeInCells.x * cellSize, room.sizeInCells.y * cellSize);
        }

        private bool[,] BuildOccupied(RectInt area)
        {
            bool[,] occupied = new bool[area.width, area.height];

            for (int y = 0; y < area.height; y++)
            {
                for (int x = 0; x < area.width; x++)
                {
                    occupied[x, y] = mapGenerator.IsWall(new Vector3Int(area.x + x, area.y + y, 0));
                }
            }

            return occupied;
        }

        private HashSet<Vector2Int> KeepClear(int roomIndex, RectInt area)
        {
            HashSet<Vector2Int> result = new HashSet<Vector2Int>();
            int cellSize = config.ChunkSize;
            int opening = Mathf.Clamp(config.OpeningWidth, 1, Mathf.Max(1, cellSize - 2));
            int offset = (cellSize - opening) / 2;

            List<RoomConnection> connections = mapGenerator.Connections;
            for (int i = 0; i < connections.Count; i++)
            {
                RoomConnection connection = connections[i];
                if (connection.roomA != roomIndex && connection.roomB != roomIndex)
                {
                    continue;
                }

                int ox = connection.cell.x * cellSize;
                int oy = connection.cell.y * cellSize;
                RectInt carve;

                if (connection.side == RoomSide.Right)
                {
                    carve = new RectInt(ox + cellSize - 1, oy + offset, 2, opening);
                }
                else
                {
                    carve = new RectInt(ox + offset, oy + cellSize - 1, opening, 2);
                }

                for (int y = carve.y - 1; y <= carve.y + carve.height; y++)
                {
                    for (int x = carve.x - 1; x <= carve.x + carve.width; x++)
                    {
                        result.Add(new Vector2Int(x - area.x, y - area.y));
                    }
                }
            }

            return result;
        }

        private void PlaceBarriers(ZoneThemeSO theme, RectInt area, bool[,] occupied, HashSet<Vector2Int> keepClear, List<Vector2Int> footprints, System.Random rng)
        {
            int count = RangeInt(theme.BarriersMin, theme.BarriersMax, rng);

            for (int i = 0; i < count; i++)
            {
                PropDefinitionSO prop = PickWeighted(theme.BarrierBank, rng);
                if (prop == null || prop.Prefab == null)
                {
                    break;
                }

                if (!TryFindAnchor(area, prop.Footprint, occupied, keepClear, true, rng, 0, out Vector2Int anchor))
                {
                    continue;
                }

                bool[,] snapshot = (bool[,])occupied.Clone();
                MarkFootprint(occupied, anchor, prop.Footprint, true);

                if (!IsConnected(occupied))
                {
                    occupied = Restore(occupied, snapshot);
                    continue;
                }

                Vector3 position = FootprintCenter(area, anchor, prop.Footprint);
                Instantiate(prop.Prefab, position, Quaternion.identity);
                footprints.Add(anchor);
            }
        }

        private void PlaceProps(ZoneThemeSO theme, RectInt area, bool[,] occupied, HashSet<Vector2Int> keepClear, System.Random rng)
        {
            int count = RangeInt(theme.PropsMin, theme.PropsMax, rng);

            for (int i = 0; i < count; i++)
            {
                PropDefinitionSO prop = PickWeighted(theme.PropBank, rng);
                if (prop == null || prop.Prefab == null)
                {
                    break;
                }

                HashSet<Vector2Int> avoid = prop.AvoidPassages ? keepClear : null;
                if (!TryFindAnchor(area, prop.Footprint, occupied, avoid, false, rng, propEdgeMargin, out Vector2Int anchor))
                {
                    continue;
                }

                MarkFootprint(occupied, anchor, prop.Footprint, true);
                Vector3 position = FootprintCenter(area, anchor, prop.Footprint);
                GameObject instance = Instantiate(prop.Prefab, position, Quaternion.identity);
                ClampPropScale(instance);
            }
        }

        private void SpawnMiniBoss(RoomPlacement room)
        {
            if (config == null || config.MinibossPrefab == null || mapGenerator == null)
            {
                return;
            }
        
            Vector3 world = mapGenerator.TargetTilemap.GetCellCenterWorld(mapGenerator.RoomCenterCell(room));
            GameObject go = Instantiate(config.MinibossPrefab, world, Quaternion.identity);
            MiniBossController mb = go.GetComponent<MiniBossController>();
            if (mb != null) mb.Setup(config.MinibossRewardAbility, null, config.MinibossOutro);
            SpawnMinibossTrigger(room, go);
        }

        private void SpawnMinibossTrigger(RoomPlacement room, GameObject miniboss)
        {
            int cellSize = config.ChunkSize;
            Vector2Int sizeTiles = room.sizeInCells * cellSize;
            Vector3Int originCell = mapGenerator.ChunkToCell(room.originCell);
            Vector3 center = mapGenerator.TargetTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + sizeTiles.x / 2, originCell.y + sizeTiles.y / 2, 0));

            var go = new GameObject("MinibossArenaTrigger");
            go.transform.position = center;
            int bossLayer = LayerMask.NameToLayer("BossTrigger");
            if (bossLayer >= 0) go.layer = bossLayer;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(sizeTiles.x, sizeTiles.y);
            var trigger = go.AddComponent<MiniBossArenaTrigger>();
            trigger.Setup(config.MinibossIntro, miniboss);
        }

        private void SpawnMinibossTint(RoomPlacement room)
        {
            if (config == null || mapGenerator == null) return;

            int cellSize = config.ChunkSize;
            Vector2Int sizeTiles = room.sizeInCells * cellSize;
            Vector3Int originCell = mapGenerator.ChunkToCell(room.originCell);
            Vector3 center = mapGenerator.TargetTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + sizeTiles.x / 2, originCell.y + sizeTiles.y / 2, 0));

            if (whiteSprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            }

            var go = new GameObject("MinibossRoomTint");
            go.transform.position = center;
            go.transform.localScale = new Vector3(sizeTiles.x, sizeTiles.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = config.MinibossRoomTint;
            sr.sortingOrder = 1;
        }
        private void SpawnMerchant(RoomPlacement room)
        {
            if (config == null || config.MerchantPrefab == null || mapGenerator == null)
            {
                return;
            }
        
            Vector3 world = mapGenerator.TargetTilemap.GetCellCenterWorld(mapGenerator.RoomCenterCell(room));
            Instantiate(config.MerchantPrefab, world, Quaternion.identity);
            SpawnMerchantBlocker(room);
        }
        
        private void SpawnMerchantBlocker(RoomPlacement room)
        {
            int layer = LayerMask.NameToLayer("MerchantBlock");
            if (layer < 0) return;

            int playerLayer = LayerMask.NameToLayer("Player");
            int enemyLayer = LayerMask.NameToLayer("Enemies");
            if (playerLayer >= 0) Physics2D.IgnoreLayerCollision(layer, playerLayer, true);
            if (enemyLayer >= 0) Physics2D.IgnoreLayerCollision(layer, enemyLayer, false);
            Physics2D.IgnoreLayerCollision(layer, LayerMask.NameToLayer("Default"), true);
        
            int cellSize = config.ChunkSize;
            Vector2Int sizeTiles = room.sizeInCells * cellSize;
            Vector3Int originCell = mapGenerator.ChunkToCell(room.originCell);
            Vector3 center = mapGenerator.TargetTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + sizeTiles.x / 2, originCell.y + sizeTiles.y / 2, 0));
        
            var blocker = new GameObject("MerchantBlocker");
            blocker.transform.position = center;
            blocker.layer = layer;
            var rb = blocker.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var col = blocker.AddComponent<BoxCollider2D>();
            col.size = new Vector2(sizeTiles.x, sizeTiles.y);
            col.isTrigger = false;
        }
        private void SpawnEnemies(RoomPlacement room, ZoneThemeSO theme, RectInt area, bool[,] occupied, System.Random rng, int playerLevel, int worldIndex)
        {
            if (spawner == null || room.chunk == null)
            {
                return;
            }

            EnemySpawnPoint[] points = room.chunk.EnemySpawns;
            if (points == null || points.Length == 0)
            {
                return;
            }

            int maxTier = 1;
            float density = 1f;
            int baseMin = theme.EnemiesMin;
            int baseMax = theme.EnemiesMax;
            DifficultyProfileSO difficulty = config.Difficulty;

            if (difficulty != null)
            {
                DensityEntry entry = difficulty.Resolve(playerLevel);
                maxTier = Mathf.Max(1, entry.maxTier);
                density = Mathf.Max(0.1f, entry.densityMultiplier);
                baseMin = entry.enemiesMin;
                baseMax = entry.enemiesMax;
            }

            density *= 1f + 0.05f * DistanceBand(room);

            List<WeightedEnemy> allowed = FilterByTier(theme.EnemyBank, maxTier);
            if (allowed.Count == 0)
            {
                return;
            }

            int count = Mathf.RoundToInt(RangeInt(baseMin, baseMax, rng) * density);
            if (difficulty != null)
            {
                count = Mathf.Min(count, difficulty.MaxEnemiesForRoom(room.sizeInCells));
            }

            count = Mathf.Min(count, points.Length);

            List<int> available = new List<int>();
            for (int i = 0; i < points.Length; i++)
            {
                Vector3Int cell = new Vector3Int(area.x + points[i].localPosition.x, area.y + points[i].localPosition.y, 0);
                int lx = cell.x - area.x;
                int ly = cell.y - area.y;
                if (lx < 0 || ly < 0 || lx >= area.width || ly >= area.height || occupied[lx, ly])
                {
                    continue;
                }

                available.Add(i);
            }

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int pick = rng.Next(available.Count);
                int pointIndex = available[pick];
                available.RemoveAt(pick);

                WeightedEnemy enemy = PickWeighted(allowed, rng);
                if (enemy.archetype == null || enemy.archetype.Prefab == null)
                {
                    continue;
                }

                Vector3Int cell = new Vector3Int(area.x + points[pointIndex].localPosition.x, area.y + points[pointIndex].localPosition.y, 0);
                Vector3 position = mapGenerator.TargetTilemap.GetCellCenterWorld(cell);
                spawner.TrySpawn(enemy.archetype.Prefab, position, rng, out _);
            }
        }

        private int DistanceBand(RoomPlacement room)
        {
            RoomPlacement start = mapGenerator.StartRoom;
            return Mathf.Abs(room.originCell.x - start.originCell.x) + Mathf.Abs(room.originCell.y - start.originCell.y);
        }

        private static List<WeightedEnemy> FilterByTier(List<WeightedEnemy> bank, int maxTier)
        {
            List<WeightedEnemy> result = new List<WeightedEnemy>();
            if (bank == null)
            {
                return result;
            }

            int lowest = int.MaxValue;
            for (int i = 0; i < bank.Count; i++)
            {
                EnemyArchetypeSO archetype = bank[i].archetype;
                if (archetype != null)
                {
                    lowest = Mathf.Min(lowest, archetype.Tier);
                }
            }

            for (int i = 0; i < bank.Count; i++)
            {
                EnemyArchetypeSO archetype = bank[i].archetype;
                if (archetype == null)
                {
                    continue;
                }

                if (archetype.Tier <= maxTier || archetype.Tier == lowest)
                {
                    result.Add(bank[i]);
                }
            }

            return result;
        }

        private bool TryFindAnchor(RectInt area, Vector2Int footprint, bool[,] occupied, HashSet<Vector2Int> avoid, bool preferBorder, System.Random rng, int edgeMargin, out Vector2Int anchor)
        {
            anchor = Vector2Int.zero;
            footprint = new Vector2Int(Mathf.Max(1, footprint.x), Mathf.Max(1, footprint.y));

            List<Vector2Int> valid = new List<Vector2Int>();
            List<Vector2Int> border = new List<Vector2Int>();

            for (int y = edgeMargin; y <= area.height - footprint.y - edgeMargin; y++)
            {
                for (int x = edgeMargin; x <= area.width - footprint.x - edgeMargin; x++)
                {
                    bool ok = true;
                    bool touchesWall = false;

                    for (int fy = 0; fy < footprint.y && ok; fy++)
                    {
                        for (int fx = 0; fx < footprint.x && ok; fx++)
                        {
                            int gx = x + fx;
                            int gy = y + fy;

                            if (occupied[gx, gy])
                            {
                                ok = false;
                                break;
                            }

                            if (avoid != null && avoid.Contains(new Vector2Int(gx, gy)))
                            {
                                ok = false;
                                break;
                            }
                        }
                    }

                    if (!ok)
                    {
                        continue;
                    }

                    if (preferBorder)
                    {
                        for (int fy = 0; fy < footprint.y && !touchesWall; fy++)
                        {
                            for (int fx = 0; fx < footprint.x && !touchesWall; fx++)
                            {
                                int gx = x + fx;
                                int gy = y + fy;
                                if (gx == 0 || gy == 0 || gx == area.width - 1 || gy == area.height - 1 ||
                                    occupied[gx - 1, gy] || occupied[gx + 1, gy] || occupied[gx, gy - 1] || occupied[gx, gy + 1])
                                {
                                    touchesWall = true;
                                }
                            }
                        }
                    }

                    valid.Add(new Vector2Int(x, y));
                    if (touchesWall)
                    {
                        border.Add(new Vector2Int(x, y));
                    }
                }
            }

            List<Vector2Int> pool = preferBorder && border.Count > 0 ? border : valid;
            if (pool.Count == 0)
            {
                return false;
            }

            anchor = pool[rng.Next(pool.Count)];
            return true;
        }

        private void ClampPropScale(GameObject instance)
        {
            if (instance == null || propMaxSize <= 0f) return;
            SpriteRenderer sr = instance.GetComponentInChildren<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;

            Vector2 size = sr.sprite.bounds.size;
            float maxDim = Mathf.Max(size.x, size.y);
            if (maxDim > propMaxSize)
            {
                instance.transform.localScale *= propMaxSize / maxDim;
            }
        }

        private Vector3 FootprintCenter(RectInt area, Vector2Int anchor, Vector2Int footprint)
        {
            Vector3 center = mapGenerator.TargetTilemap.GetCellCenterWorld(new Vector3Int(area.x + anchor.x, area.y + anchor.y, 0));
            return center + new Vector3((footprint.x - 1) * 0.5f, (footprint.y - 1) * 0.5f, 0f);
        }

        private static void MarkFootprint(bool[,] occupied, Vector2Int anchor, Vector2Int footprint, bool value)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                for (int x = 0; x < footprint.x; x++)
                {
                    occupied[anchor.x + x, anchor.y + y] = value;
                }
            }
        }

        private static bool[,] Restore(bool[,] target, bool[,] snapshot)
        {
            System.Array.Copy(snapshot, target, snapshot.Length);
            return target;
        }

        private static bool IsConnected(bool[,] occupied)
        {
            int w = occupied.GetLength(0);
            int h = occupied.GetLength(1);

            int total = 0;
            int startX = -1;
            int startY = -1;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!occupied[x, y])
                    {
                        total++;
                        if (startX < 0)
                        {
                            startX = x;
                            startY = y;
                        }
                    }
                }
            }

            if (total == 0)
            {
                return false;
            }

            bool[,] seen = new bool[w, h];
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(startX, startY));
            seen[startX, startY] = true;
            int visited = 0;

            while (stack.Count > 0)
            {
                Vector2Int c = stack.Pop();
                visited++;

                TryPush(seen, occupied, stack, c.x + 1, c.y, w, h);
                TryPush(seen, occupied, stack, c.x - 1, c.y, w, h);
                TryPush(seen, occupied, stack, c.x, c.y + 1, w, h);
                TryPush(seen, occupied, stack, c.x, c.y - 1, w, h);
            }

            return visited == total;
        }

        private static void TryPush(bool[,] seen, bool[,] occupied, Stack<Vector2Int> stack, int x, int y, int w, int h)
        {
            if (x < 0 || y < 0 || x >= w || y >= h || seen[x, y] || occupied[x, y])
            {
                return;
            }

            seen[x, y] = true;
            stack.Push(new Vector2Int(x, y));
        }

        private static int RangeInt(int min, int max, System.Random rng)
        {
            if (max < min)
            {
                max = min;
            }

            return rng.Next(min, max + 1);
        }

        private static PropDefinitionSO PickWeighted(List<WeightedProp> bank, System.Random rng)
        {
            if (bank == null || bank.Count == 0)
            {
                return null;
            }

            int total = 0;
            for (int i = 0; i < bank.Count; i++)
            {
                if (bank[i].prop != null)
                {
                    total += Mathf.Max(0, bank[i].weight);
                }
            }

            if (total <= 0)
            {
                return null;
            }

            int pick = rng.Next(total);
            for (int i = 0; i < bank.Count; i++)
            {
                if (bank[i].prop == null)
                {
                    continue;
                }

                pick -= Mathf.Max(0, bank[i].weight);
                if (pick < 0)
                {
                    return bank[i].prop;
                }
            }

            return null;
        }

        private static WeightedEnemy PickWeighted(List<WeightedEnemy> bank, System.Random rng)
        {
            WeightedEnemy fallback = default;

            if (bank == null || bank.Count == 0)
            {
                return fallback;
            }

            int total = 0;
            for (int i = 0; i < bank.Count; i++)
            {
                if (bank[i].archetype != null)
                {
                    total += Mathf.Max(0, bank[i].weight);
                    fallback = bank[i];
                }
            }

            if (total <= 0)
            {
                return fallback;
            }

            int pick = rng.Next(total);
            for (int i = 0; i < bank.Count; i++)
            {
                if (bank[i].archetype == null)
                {
                    continue;
                }

                pick -= Mathf.Max(0, bank[i].weight);
                if (pick < 0)
                {
                    return bank[i];
                }
            }

            return fallback;
        }
    }
}
