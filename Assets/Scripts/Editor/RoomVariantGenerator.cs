using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Genera variantes de cuartos a partir de plantillas: garantiza aperturas de 4 tiles en el
    /// centro de cada borde de cada celda (6..9), valida los grafos de piso y agua, agrega pozos
    /// de agua en las variantes B/C, pinta los tiles por rol y escribe los MapChunkSO.
    /// </summary>
    public static class RoomVariantGenerator
    {
        private const int CellSize = 16;

        /// <summary>
        /// Genera <paramref name="variantsPerSize"/> variantes por tamaño y los cuartos especiales,
        /// y cablea el banco/mundo indicados.
        /// </summary>
        public static void Generate(
            TilesetCatalogSO catalog,
            TilePainterProfileSO profile,
            string templatesFolder,
            string outputFolder,
            string bankPath,
            WorldConfigSO world,
            int seed,
            int variantsPerSize,
            List<Vector2Int> sizes)
        {
            if (catalog == null || profile == null)
            {
                Debug.LogError("RoomVariantGenerator: faltan catálogo o perfil de pintado.");
                return;
            }

            if (string.IsNullOrEmpty(outputFolder)) outputFolder = "Assets/ScriptableObjects/Rooms/Sea/Generated";
            EnsureFolder(outputFolder);

            if (sizes == null || sizes.Count == 0)
            {
                sizes = new List<Vector2Int>
                {
                    new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2),
                    new Vector2Int(2, 2), new Vector2Int(3, 3)
                };
            }

            variantsPerSize = Mathf.Max(1, variantsPerSize);
            var templates = LoadTemplates(templatesFolder);
            var normals = new List<MapChunkSO>();
            int index = 0;

            foreach (var size in sizes)
            {
                var sizeTemplates = new List<MapChunkSO>();
                for (int i = 0; i < templates.Count; i++)
                {
                    var t = templates[i];
                    if (t != null && t.Role == CellRole.Normal && t.SizeInCells == size)
                    {
                        sizeTemplates.Add(t);
                    }
                }

                for (int v = 0; v < variantsPerSize; v++)
                {
                    var template = sizeTemplates.Count > 0 ? sizeTemplates[v % sizeTemplates.Count] : FindRotatedTemplate(templates, size);
                    int variantSeed = seed + index * 101;

                    TerrainType[] grid = BuildGrid(template, size, catalog, profile);
                    NormalizeOpenings(grid, size, profile.OpeningWidth);
                    ApplyMutations(grid, size, v, variantSeed);
                    ApplyWaterPattern(grid, size, v, profile, out _);

                    if (!IsTerrainConnected(grid, size, t => t == TerrainType.Floor, out int floorCount) || floorCount < 16)
                    {
                        grid = BuildOpenGrid(size, profile.OpeningWidth);
                        ApplyWaterPattern(grid, size, v, profile, out _);
                    }

                    TileBase[] tiles = Paint(grid, size, profile, variantSeed);
                    EnemySpawnPoint[] spawns = BuildSpawns(grid, size);
                    string roomName = "Room_Sea_" + size.x + "x" + size.y + "_" + VariantLetter(v);
                    normals.Add(CreateRoom(roomName, size, tiles, spawns, RoomTag.Sea, CellRole.Normal, outputFolder));
                    index++;
                }
            }

            var start = BuildSpecial(templates, "Start_Sea", new Vector2Int(1, 1), CellRole.Start, outputFolder, catalog, profile, seed + 900);
            var boss = BuildSpecial(templates, "Boss_Sea", new Vector2Int(3, 3), CellRole.Boss, outputFolder, catalog, profile, seed + 901);
            var merchant = BuildSpecial(templates, "Room_Merchant_2x2_Sea", new Vector2Int(2, 2), CellRole.Normal, outputFolder, catalog, profile, seed + 902);
            var miniboss = BuildSpecial(templates, "Room_Miniboss_2x2_Sea", new Vector2Int(2, 2), CellRole.Normal, outputFolder, catalog, profile, seed + 903);

            BlockBankSO bank = null;
            if (!string.IsNullOrEmpty(bankPath))
            {
                EnsureFolder(ParentFolder(bankPath));
                bank = AssetDatabase.LoadAssetAtPath<BlockBankSO>(bankPath);
                if (bank == null)
                {
                    bank = ScriptableObject.CreateInstance<BlockBankSO>();
                    bank.name = "Bank_Sea_Generated";
                    AssetDatabase.CreateAsset(bank, bankPath);
                }
            }

            if (bank != null)
            {
                var so = new SerializedObject(bank);
                var normalProp = so.FindProperty("normalChunks");
                normalProp.arraySize = normals.Count;
                for (int i = 0; i < normals.Count; i++)
                {
                    normalProp.GetArrayElementAtIndex(i).objectReferenceValue = normals[i];
                }

                so.FindProperty("startRoom").objectReferenceValue = start;
                so.FindProperty("bossArena").objectReferenceValue = boss;
                so.FindProperty("merchantRoom").objectReferenceValue = merchant;
                so.FindProperty("minibossRoom").objectReferenceValue = miniboss;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bank);
            }

            if (world != null && bank != null)
            {
                var wso = new SerializedObject(world);
                wso.FindProperty("bank").objectReferenceValue = bank;
                wso.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(world);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"RoomVariantGenerator: {normals.Count} normales + 4 especiales en '{outputFolder}'. Banco: {bank}", bank);
        }

        private static List<MapChunkSO> LoadTemplates(string folder)
        {
            var list = new List<MapChunkSO>();
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder)) return list;

            foreach (string guid in AssetDatabase.FindAssets("t:MapChunkSO", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Generated/")) continue;

                var room = AssetDatabase.LoadAssetAtPath<MapChunkSO>(path);
                if (room != null) list.Add(room);
            }

            return list;
        }

        private static MapChunkSO FindRotatedTemplate(List<MapChunkSO> templates, Vector2Int size)
        {
            var swapped = new Vector2Int(size.y, size.x);
            for (int i = 0; i < templates.Count; i++)
            {
                var t = templates[i];
                if (t != null && t.Role == CellRole.Normal && t.SizeInCells == swapped)
                {
                    return t;
                }
            }

            return null;
        }

        private static TerrainType[] BuildGrid(MapChunkSO template, Vector2Int cells, TilesetCatalogSO catalog, TilePainterProfileSO profile)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;

            if (template == null || template.Tiles == null)
            {
                return BuildOpenGrid(cells, profile.OpeningWidth);
            }

            if (template.Size.x == w && template.Size.y == h)
            {
                var grid = new TerrainType[w * h];
                for (int i = 0; i < grid.Length && i < template.Tiles.Length; i++)
                {
                    grid[i] = Classify(template.Tiles[i], catalog, profile);
                }
                return grid;
            }

            // Plantilla transpuesta (p. ej. 2x1 -> 1x2): se rota 90° en sentido horario.
            int sw = template.Size.x;
            int sh = template.Size.y;
            var source = new TerrainType[sw * sh];
            for (int i = 0; i < source.Length && i < template.Tiles.Length; i++)
            {
                source[i] = Classify(template.Tiles[i], catalog, profile);
            }

            var rotated = new TerrainType[w * h];
            for (int sy = 0; sy < sh; sy++)
            {
                for (int sx = 0; sx < sw; sx++)
                {
                    int nx = sh - 1 - sy;
                    int ny = sx;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    rotated[ny * w + nx] = source[sy * sw + sx];
                }
            }

            return rotated;
        }

        private static TerrainType Classify(TileBase tile, TilesetCatalogSO catalog, TilePainterProfileSO profile)
        {
            if (tile == null)
            {
                return TerrainType.Floor;
            }

            TerrainType type = catalog.GetTerrain(tile);
            if (type == TerrainType.Void)
            {
                return TerrainType.Floor;
            }

            if (type == TerrainType.Water && !profile.HasWater)
            {
                return TerrainType.Floor;
            }

            return type;
        }

        private static TerrainType[] BuildOpenGrid(Vector2Int cells, int openingWidth)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            var grid = new TerrainType[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    grid[y * w + x] = x == 0 || y == 0 || x == w - 1 || y == h - 1
                        ? TerrainType.Wall
                        : TerrainType.Floor;
                }
            }

            NormalizeOpenings(grid, cells, openingWidth);
            return grid;
        }

        private static void NormalizeOpenings(TerrainType[] grid, Vector2Int cells, int openingWidth)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            int opening = Mathf.Clamp(openingWidth, 1, CellSize - 2);
            int offset = (CellSize - opening) / 2;

            // El anillo completo debe ser muro; después se abren los 4 tiles centrales de cada borde.
            for (int x = 0; x < w; x++)
            {
                grid[x] = TerrainType.Wall;
                grid[(h - 1) * w + x] = TerrainType.Wall;
            }

            for (int y = 0; y < h; y++)
            {
                grid[y * w] = TerrainType.Wall;
                grid[y * w + w - 1] = TerrainType.Wall;
            }

            for (int cx = 0; cx < cells.x; cx++)
            {
                int x0 = cx * CellSize + offset;
                for (int i = 0; i < opening; i++)
                {
                    int x = x0 + i;
                    SetFloor(grid, w, h, x, 0);
                    SetFloor(grid, w, h, x, h - 1);
                }
            }

            for (int cy = 0; cy < cells.y; cy++)
            {
                int y0 = cy * CellSize + offset;
                for (int i = 0; i < opening; i++)
                {
                    int y = y0 + i;
                    SetFloor(grid, w, h, 0, y);
                    SetFloor(grid, w, h, w - 1, y);
                }
            }
        }

        private static void SetFloor(TerrainType[] grid, int w, int h, int x, int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            grid[y * w + x] = TerrainType.Floor;
        }

        private static void ApplyMutations(TerrainType[] grid, Vector2Int cells, int variant, int seed)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            var rng = new System.Random(seed + variant * 7919);
            int style = variant % 3;
            int target = style == 0 ? 1 : style == 1 ? 3 : 4;

            for (int i = 0; i < target; i++)
            {
                TerrainType[] snapshot = (TerrainType[])grid.Clone();
                bool add = style != 2 || rng.Next(100) < 55;

                if (add)
                {
                    int bw = 1 + rng.Next(2);
                    int bh = 1 + rng.Next(2);
                    TryAddBlock(grid, w, h, rng, bw, bh);
                }
                else
                {
                    TryRemoveWall(grid, w, h, rng);
                }

                if (!IsTerrainConnected(grid, cells, t => t == TerrainType.Floor, out _))
                {
                    System.Array.Copy(snapshot, grid, snapshot.Length);
                }
            }
        }

        private static void TryAddBlock(TerrainType[] grid, int w, int h, System.Random rng, int bw, int bh)
        {
            if (w <= bw + 4 || h <= bh + 4) return;

            int x = 2 + rng.Next(w - bw - 4);
            int y = 2 + rng.Next(h - bh - 4);

            for (int fy = 0; fy < bh; fy++)
            {
                for (int fx = 0; fx < bw; fx++)
                {
                    grid[(y + fy) * w + (x + fx)] = TerrainType.Wall;
                }
            }
        }

        private static void TryRemoveWall(TerrainType[] grid, int w, int h, System.Random rng)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int x = 2 + rng.Next(Mathf.Max(1, w - 4));
                int y = 2 + rng.Next(Mathf.Max(1, h - 4));
                int index = y * w + x;
                if (grid[index] == TerrainType.Wall)
                {
                    grid[index] = TerrainType.Floor;
                    return;
                }
            }
        }

        /// <summary>
        /// Aplica un pozo de agua a las variantes B/C. La variante B usa un pozo central; la C, un
        /// pozo lateral. Si el pozo rompe el piso, el agua o la unión piso|agua, se revierte.
        /// </summary>
        private static void ApplyWaterPattern(
            TerrainType[] grid,
            Vector2Int cells,
            int variant,
            TilePainterProfileSO profile,
            out bool applied)
        {
            applied = false;
            if (profile == null || !profile.HasWater || variant <= 0)
            {
                return;
            }

            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            bool single = cells.x == 1 && cells.y == 1;
            int pool = single ? 6
                : (cells.x >= 3 && cells.y >= 3 ? 12
                    : (cells.x >= 2 && cells.y >= 2 ? 10 : 8));
            int x0;
            int y0;

            if (variant % 2 == 1)
            {
                x0 = w / 2 - pool / 2;
                y0 = h / 2 - pool / 2;
            }
            else if (w >= 32)
            {
                x0 = w - pool - 3;
                y0 = h / 2 - pool / 2;
            }
            else
            {
                x0 = 3;
                y0 = 3;
            }

            x0 = Mathf.Clamp(x0, 2, Mathf.Max(2, w - pool - 2));
            y0 = Mathf.Clamp(y0, 2, Mathf.Max(2, h - pool - 2));

            TerrainType[] snapshot = (TerrainType[])grid.Clone();
            int waterCount = 0;

            for (int y = y0; y < y0 + pool && y < h; y++)
            {
                for (int x = x0; x < x0 + pool && x < w; x++)
                {
                    int index = y * w + x;
                    if (grid[index] == TerrainType.Floor)
                    {
                        grid[index] = TerrainType.Water;
                        waterCount++;
                    }
                }
            }

            if (waterCount == 0 ||
                !IsTerrainConnected(grid, cells, t => t == TerrainType.Floor, out _) ||
                !IsTerrainConnected(grid, cells, t => t == TerrainType.Water, out _) ||
                !IsTerrainConnected(grid, cells, t => t == TerrainType.Floor || t == TerrainType.Water, out _))
            {
                System.Array.Copy(snapshot, grid, snapshot.Length);
                return;
            }

            applied = true;
        }

        private static bool IsTerrainConnected(TerrainType[] grid, Vector2Int cells, System.Func<TerrainType, bool> walkable, out int count)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            count = 0;
            int start = -1;

            for (int i = 0; i < grid.Length; i++)
            {
                if (walkable(grid[i]))
                {
                    count++;
                    if (start < 0) start = i;
                }
            }

            if (start < 0) return false;

            var seen = new bool[grid.Length];
            var stack = new Stack<int>();
            stack.Push(start);
            seen[start] = true;
            int visited = 0;

            while (stack.Count > 0)
            {
                int index = stack.Pop();
                visited++;
                int x = index % w;
                int y = index / w;

                PushWalkable(seen, grid, walkable, stack, w, h, x + 1, y);
                PushWalkable(seen, grid, walkable, stack, w, h, x - 1, y);
                PushWalkable(seen, grid, walkable, stack, w, h, x, y + 1);
                PushWalkable(seen, grid, walkable, stack, w, h, x, y - 1);
            }

            return visited == count;
        }

        private static void PushWalkable(
            bool[] seen,
            TerrainType[] grid,
            System.Func<TerrainType, bool> walkable,
            Stack<int> stack,
            int w,
            int h,
            int x,
            int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            int index = y * w + x;
            if (seen[index] || !walkable(grid[index])) return;

            seen[index] = true;
            stack.Push(index);
        }

        private static TileBase[] Paint(TerrainType[] grid, Vector2Int cells, TilePainterProfileSO profile, int seed)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            var tiles = new TileBase[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;
                    TerrainType terrain = grid[index];

                    if (terrain == TerrainType.Wall)
                    {
                        bool wallAbove = y + 1 < h && grid[index + w] == TerrainType.Wall;
                        bool wallBelow = y > 0 && grid[index - w] == TerrainType.Wall;
                        bool topExposed = !wallAbove;
                        bool cap = topExposed && wallBelow;
                        tiles[index] = profile.PickWall(cap, topExposed, x, y, seed);
                    }
                    else if (terrain == TerrainType.Water)
                    {
                        tiles[index] = profile.PickWater(x, y, seed);
                    }
                    else
                    {
                        bool solidAbove = y + 1 < h && (grid[index + w] == TerrainType.Wall || grid[index + w] == TerrainType.Water);
                        bool solidBelow = y > 0 && (grid[index - w] == TerrainType.Wall || grid[index - w] == TerrainType.Water);
                        bool solidLeft = x > 0 && (grid[index - 1] == TerrainType.Wall || grid[index - 1] == TerrainType.Water);
                        bool solidRight = x + 1 < w && (grid[index + 1] == TerrainType.Wall || grid[index + 1] == TerrainType.Water);
                        tiles[index] = profile.PickFloor(solidAbove, solidBelow, solidLeft, solidRight, x, y, seed);
                    }
                }
            }

            return tiles;
        }

        private static EnemySpawnPoint[] BuildSpawns(TerrainType[] grid, Vector2Int cells)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            var points = new List<EnemySpawnPoint>();
            var used = new HashSet<Vector2Int>();
            int n = 3;
            int x0 = 4;
            int y0 = 4;
            int x1 = Mathf.Max(x0, w - 5);
            int y1 = Mathf.Max(y0, h - 5);

            for (int iy = 0; iy < n; iy++)
            {
                for (int ix = 0; ix < n; ix++)
                {
                    float tx = n == 1 ? 0.5f : ix / (float)(n - 1);
                    float ty = n == 1 ? 0.5f : iy / (float)(n - 1);
                    int px = Mathf.RoundToInt(Mathf.Lerp(x0, x1, tx));
                    int py = Mathf.RoundToInt(Mathf.Lerp(y0, y1, ty));

                    if (!TryFindNonWall(grid, w, h, px, py, 4, out int fx, out int fy)) continue;

                    var cell = new Vector2Int(fx, fy);
                    if (!used.Add(cell)) continue;

                    points.Add(new EnemySpawnPoint
                    {
                        localPosition = cell,
                        weight = 1,
                        terrain = grid[fy * w + fx]
                    });
                }
            }

            for (int y = 2; y < h - 2; y++)
            {
                for (int x = 2; x < w - 2; x++)
                {
                    if (grid[y * w + x] != TerrainType.Water) continue;

                    var cell = new Vector2Int(x, y);
                    if (!used.Add(cell)) continue;

                    points.Add(new EnemySpawnPoint
                    {
                        localPosition = cell,
                        weight = 1,
                        terrain = TerrainType.Water
                    });
                    return points.ToArray();
                }
            }

            return points.ToArray();
        }

        private static bool TryFindNonWall(TerrainType[] grid, int w, int h, int x, int y, int radius, out int resultX, out int resultY)
        {
            resultX = x;
            resultY = y;

            for (int r = 0; r <= radius; r++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;

                        int tx = x + dx;
                        int ty = y + dy;
                        if (tx < 1 || ty < 1 || tx >= w - 1 || ty >= h - 1) continue;
                        if (grid[ty * w + tx] != TerrainType.Wall)
                        {
                            resultX = tx;
                            resultY = ty;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static MapChunkSO BuildSpecial(
            List<MapChunkSO> templates,
            string templateName,
            Vector2Int size,
            CellRole role,
            string outputFolder,
            TilesetCatalogSO catalog,
            TilePainterProfileSO profile,
            int seed)
        {
            MapChunkSO template = null;
            for (int i = 0; i < templates.Count; i++)
            {
                if (templates[i] != null && templates[i].name == templateName)
                {
                    template = templates[i];
                    break;
                }
            }

            if (template == null)
            {
                for (int i = 0; i < templates.Count; i++)
                {
                    if (templates[i] != null && templates[i].SizeInCells == size && templates[i].Role == role)
                    {
                        template = templates[i];
                        break;
                    }
                }
            }

            TerrainType[] grid = BuildGrid(template, size, catalog, profile);
            NormalizeOpenings(grid, size, profile.OpeningWidth);

            if (!IsTerrainConnected(grid, size, t => t == TerrainType.Floor, out int floorCount) || floorCount < 16)
            {
                grid = BuildOpenGrid(size, profile.OpeningWidth);
            }

            if (role == CellRole.Boss)
            {
                ApplyWaterPattern(grid, size, 2, profile, out _);
            }

            TileBase[] tiles = Paint(grid, size, profile, seed);
            EnemySpawnPoint[] spawns = role == CellRole.Normal ? BuildSpawns(grid, size) : new EnemySpawnPoint[0];
            string roomName = "Room_Sea_" + templateName.Replace("Room_", string.Empty) + "_Gen";
            return CreateRoom(roomName, size, tiles, spawns, RoomTag.Sea, role, outputFolder);
        }

        private static MapChunkSO CreateRoom(
            string name,
            Vector2Int cells,
            TileBase[] tiles,
            EnemySpawnPoint[] spawns,
            RoomTag tag,
            CellRole role,
            string folder)
        {
            int w = cells.x * CellSize;
            int h = cells.y * CellSize;
            string path = folder + "/" + name + ".asset";
            var room = AssetDatabase.LoadAssetAtPath<MapChunkSO>(path);
            if (room == null)
            {
                room = ScriptableObject.CreateInstance<MapChunkSO>();
                AssetDatabase.CreateAsset(room, path);
            }

            room.name = name;
            room.SetSize(new Vector2Int(w, h));
            room.SetTileData(tiles);
            room.SetSizeInCells(cells);
            room.SetTag(tag);
            room.SetRole(role);
            room.SetEnemySpawns(spawns);

            EditorUtility.SetDirty(room);
            return room;
        }

        private static string VariantLetter(int variant)
        {
            return ((char)('A' + variant % 26)).ToString();
        }

        private static string ParentFolder(string path)
        {
            int slash = path.LastIndexOf('/');
            return slash > 0 ? path.Substring(0, slash) : "Assets";
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            string parent = ParentFolder(path);
            string leaf = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
