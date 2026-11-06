using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Herramientas de editor para el sistema de terreno: crea los tiles de agua del mar,
    /// migra los hábitats de los enemigos, regenera los cuartos del mar y valida aperturas,
    /// conectividad y spawns por terreno.
    /// </summary>
    public static class TerrainTools
    {
        private const int CellSize = 16;
        private const int OpeningWidth = 4;

        [MenuItem("Tools/Madness Dreams/Terrain/Build Sea Water")]
        public static void BuildSeaWater()
        {
            TilesetCatalogSO catalog = FindAsset<TilesetCatalogSO>("Tileset_Sea");
            TilePainterProfileSO profile = FindAsset<TilePainterProfileSO>("TilePainterProfile_Sea");
            if (catalog == null)
            {
                Debug.LogError("[Terrain] No se encontró Tileset_Sea.");
                return;
            }

            string artFolder = "Assets/Art/Tilesets/Sea";
            string tilesFolder = "Assets/Tiles/Sea";
            EnsureFolder(artFolder);
            EnsureFolder(tilesFolder);

            var waterTiles = new List<TileBase>();
            for (int variant = 0; variant < 3; variant++)
            {
                Tile tile = CreateWaterTile(artFolder, tilesFolder, variant);
                if (tile != null)
                {
                    waterTiles.Add(tile);
                }
            }

            SetCatalogWater(catalog, waterTiles);
            if (profile != null)
            {
                profile.SetWaterTiles(waterTiles);
                EditorUtility.SetDirty(profile);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Terrain] Agua del mar: {waterTiles.Count} tiles. Catálogo: {catalog.name}. Perfil: {profile}", catalog);
        }

        [MenuItem("Tools/Madness Dreams/Terrain/Migrate Enemy Habitats")]
        public static void MigrateEnemyHabitats()
        {
            var definitions = new Dictionary<string, EnemyHabitat>
            {
                { "EnemyStats_Wolf", EnemyHabitat.Ground },
                { "EnemyStats_Bramble", EnemyHabitat.Ground },
                { "EnemyStats_Woodcutter", EnemyHabitat.Ground },
                { "EnemyStats_Firefly", EnemyHabitat.Flying },
                { "EnemyStats_CabinBoy", EnemyHabitat.Ground },
                { "EnemyStats_Mermaid", EnemyHabitat.Water },
                { "EnemyStats_Crocodile", EnemyHabitat.Ground | EnemyHabitat.Water },
                { "EnemyStats_DustFairy", EnemyHabitat.Flying }
            };

            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:EnemyStatsSO"))
            {
                var stats = AssetDatabase.LoadAssetAtPath<EnemyStatsSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (stats == null || !definitions.TryGetValue(stats.name, out EnemyHabitat habitat))
                {
                    continue;
                }

                bool hides = stats.name == "EnemyStats_Mermaid" || stats.name == "EnemyStats_Crocodile"
                    || stats.name == "EnemyStats_DustFairy" || stats.name == "EnemyStats_Firefly";
                TerrainType hideTerrain = stats.name == "EnemyStats_DustFairy" || stats.name == "EnemyStats_Firefly"
                    ? TerrainType.Wall
                    : TerrainType.Water;
                float hideMin = stats.name == "EnemyStats_Crocodile" ? 6f
                    : (stats.name == "EnemyStats_DustFairy" || stats.name == "EnemyStats_Firefly" ? 7f : 5f);
                float hideMax = stats.name == "EnemyStats_Crocodile" ? 10f
                    : (stats.name == "EnemyStats_DustFairy" || stats.name == "EnemyStats_Firefly" ? 12f : 8f);
                float hideDuration = stats.name == "EnemyStats_Mermaid" ? 2.5f
                    : (stats.name == "EnemyStats_DustFairy" || stats.name == "EnemyStats_Firefly" ? 1.5f : 2f);
                float outWater = stats.name == "EnemyStats_Crocodile" ? 4f : 0f;

                var so = new SerializedObject(stats);
                so.FindProperty("habitat").intValue = (int)habitat;
                so.FindProperty("passThroughWalls").boolValue = (habitat & EnemyHabitat.Flying) != 0;
                so.FindProperty("visionThroughWalls").boolValue = false;
                so.FindProperty("hidesPeriodically").boolValue = hides;
                so.FindProperty("hideIntervalMin").floatValue = hideMin;
                so.FindProperty("hideIntervalMax").floatValue = hideMax;
                so.FindProperty("hideDuration").floatValue = hideDuration;
                so.FindProperty("hideTerrain").intValue = (int)hideTerrain;
                so.FindProperty("hideSearchRadius").intValue = 10;
                so.FindProperty("maxTimeOutOfWater").floatValue = outWater;
                so.FindProperty("hideWhileNotVisible").boolValue = true;
                bool stationary = stats.name == "EnemyStats_Bramble";
                so.FindProperty("stationary").boolValue = stationary;
                so.FindProperty("immuneWhileIdle").boolValue = stationary;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stats);
                changed++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Terrain] Hábitats y comportamiento migrados en {changed} EnemyStatsSO.");
        }

        [MenuItem("Tools/Madness Dreams/Terrain/Regenerate Sea Rooms")]
        public static void RegenerateSeaRooms()
        {
            TilesetCatalogSO catalog = FindAsset<TilesetCatalogSO>("Tileset_Sea");
            TilePainterProfileSO profile = FindAsset<TilePainterProfileSO>("TilePainterProfile_Sea");
            if (catalog == null || profile == null)
            {
                Debug.LogError("[Terrain] Faltan Tileset_Sea o TilePainterProfile_Sea.");
                return;
            }

            BlockBankSO bank = FindAsset<BlockBankSO>("Bank_Sea");
            WorldConfigSO world = FindAsset<WorldConfigSO>("World_Sea");
            string bankPath = bank != null ? AssetDatabase.GetAssetPath(bank) : "Assets/ScriptableObjects/Sample/Sea/Bank_Sea.asset";

            RoomVariantGenerator.Generate(
                catalog,
                profile,
                "Assets/ScriptableObjects/Rooms/Sea",
                "Assets/ScriptableObjects/Rooms/Sea/Generated",
                bankPath,
                world,
                20240,
                3,
                null);

            RenderSeaPreviews();
        }

        [MenuItem("Tools/Madness Dreams/Terrain/Render Sea Previews")]
        public static void RenderSeaPreviews()
        {
            int count = RoomPreviewRenderer.RenderFolder(
                "Assets/ScriptableObjects/Rooms/Sea/Generated",
                "Assets/Previews/Sea",
                8);
            Debug.Log($"[Terrain] Previews renderizadas: {count}.");
        }

        [MenuItem("Tools/Madness Dreams/Terrain/Validate Terrain")]
        public static void ValidateAllWorlds()
        {
            int rooms = 0;
            int problems = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:WorldConfigSO"))
            {
                var config = AssetDatabase.LoadAssetAtPath<WorldConfigSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (config == null || config.Bank == null || config.Tileset == null)
                {
                    continue;
                }

                var chunks = new List<MapChunkSO>();
                if (config.Bank.NormalChunks != null)
                {
                    chunks.AddRange(config.Bank.NormalChunks);
                }

                AddChunk(chunks, config.Bank.StartRoom);
                AddChunk(chunks, config.Bank.BossArena);
                AddChunk(chunks, config.Bank.MerchantRoom);
                AddChunk(chunks, config.Bank.MinibossRoom);

                for (int i = 0; i < chunks.Count; i++)
                {
                    rooms++;
                    if (!ValidateChunk(chunks[i], config.Tileset))
                    {
                        problems++;
                    }
                }
            }

            if (problems == 0)
            {
                Debug.Log($"[Terrain] {rooms} cuarto(s) válidos.");
            }
            else
            {
                Debug.LogWarning($"[Terrain] {problems} de {rooms} cuarto(s) con problemas.");
            }
        }

        /// <summary>Valida un cuarto: tamaño, anillo, aperturas, conectividad y spawns por terreno.</summary>
        public static bool ValidateChunk(MapChunkSO room, TilesetCatalogSO catalog)
        {
            if (room == null)
            {
                return true;
            }

            Vector2Int size = room.Size;
            TileBase[] tiles = room.Tiles;
            var issues = new List<string>();

            if (size.x <= 0 || size.y <= 0 || tiles == null || tiles.Length != size.x * size.y)
            {
                issues.Add($"tamaño/tiles inválidos ({size}, {(tiles != null ? tiles.Length : 0)})");
                Debug.LogWarning($"[Terrain] '{room.name}': {string.Join("; ", issues)}.", room);
                return false;
            }

            if (size.x % CellSize != 0 || size.y % CellSize != 0)
            {
                issues.Add($"tamaño {size} no es múltiplo de {CellSize}");
            }

            var terrain = new TerrainType[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                terrain[i] = catalog != null
                    ? catalog.GetTerrain(tiles[i])
                    : (tiles[i] != null ? TerrainType.Floor : TerrainType.Void);
            }

            int ringIssues = 0;
            int openingIssues = 0;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int index = y * size.x + x;
                    bool opening = IsOpening(x, y, size, CellSize, OpeningWidth);

                    if (opening)
                    {
                        if (terrain[index] != TerrainType.Floor)
                        {
                            openingIssues++;
                        }
                    }
                    else if (IsRing(x, y, size) && terrain[index] != TerrainType.Wall)
                    {
                        ringIssues++;
                    }
                }
            }

            if (ringIssues > 0)
            {
                issues.Add($"{ringIssues} celdas del anillo no son muro");
            }

            if (openingIssues > 0)
            {
                issues.Add($"{openingIssues} celdas de apertura no son piso");
            }

            bool floorOk = IsTerrainConnected(terrain, size, t => t == TerrainType.Floor, out int floorCount);
            bool waterOk = IsTerrainConnected(terrain, size, t => t == TerrainType.Water, out int waterCount);
            bool unionOk = IsTerrainConnected(terrain, size, t => t == TerrainType.Floor || t == TerrainType.Water, out _);

            if (!floorOk || floorCount < 16)
            {
                issues.Add($"piso no conexo o insuficiente ({floorCount})");
            }

            if (waterCount > 0 && !waterOk)
            {
                issues.Add($"agua no conexa ({waterCount} tiles)");
            }

            if (!unionOk)
            {
                issues.Add("piso+agua no conexos (jugador no puede recorrer)");
            }

            EnemySpawnPoint[] spawns = room.EnemySpawns;
            if (spawns != null)
            {
                for (int i = 0; i < spawns.Length; i++)
                {
                    Vector2Int local = spawns[i].localPosition;
                    if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y)
                    {
                        issues.Add($"spawn {i} fuera del cuarto ({local})");
                        continue;
                    }

                    TerrainType actual = terrain[local.y * size.x + local.x];
                    if (actual == TerrainType.Wall || actual == TerrainType.Void)
                    {
                        issues.Add($"spawn {i} en {actual}");
                    }

                    if (spawns[i].terrain != TerrainType.Void && spawns[i].terrain != actual)
                    {
                        issues.Add($"spawn {i} marcado {spawns[i].terrain} pero el tile es {actual}");
                    }
                }
            }

            if (issues.Count > 0)
            {
                Debug.LogWarning($"[Terrain] '{room.name}': {issues.Count} problema(s): {string.Join("; ", issues)}.", room);
                return false;
            }

            return true;
        }

        private static bool IsTerrainConnected(TerrainType[] terrain, Vector2Int size, System.Func<TerrainType, bool> walkable, out int count)
        {
            int w = size.x;
            int h = size.y;
            count = 0;
            int start = -1;

            for (int i = 0; i < terrain.Length; i++)
            {
                if (walkable(terrain[i]))
                {
                    count++;
                    if (start < 0) start = i;
                }
            }

            if (start < 0)
            {
                return false;
            }

            var seen = new bool[terrain.Length];
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
                Push(seen, terrain, walkable, stack, w, h, x + 1, y);
                Push(seen, terrain, walkable, stack, w, h, x - 1, y);
                Push(seen, terrain, walkable, stack, w, h, x, y + 1);
                Push(seen, terrain, walkable, stack, w, h, x, y - 1);
            }

            return visited == count;
        }

        private static void Push(
            bool[] seen,
            TerrainType[] terrain,
            System.Func<TerrainType, bool> walkable,
            Stack<int> stack,
            int w,
            int h,
            int x,
            int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            int index = y * w + x;
            if (seen[index] || !walkable(terrain[index])) return;
            seen[index] = true;
            stack.Push(index);
        }

        private static bool IsRing(int x, int y, Vector2Int size)
        {
            return x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1;
        }

        private static bool IsOpening(int x, int y, Vector2Int size, int cellSize, int openingWidth)
        {
            int offset = (cellSize - openingWidth) / 2;

            if (y == 0 || y == size.y - 1)
            {
                int lx = x - (x / cellSize) * cellSize;
                if (lx >= offset && lx < offset + openingWidth) return true;
            }

            if (x == 0 || x == size.x - 1)
            {
                int ly = y - (y / cellSize) * cellSize;
                if (ly >= offset && ly < offset + openingWidth) return true;
            }

            return false;
        }

        private static void AddChunk(List<MapChunkSO> chunks, MapChunkSO chunk)
        {
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
        }

        private static Tile CreateWaterTile(string artFolder, string tilesFolder, int variant)
        {
            string pngPath = artFolder + "/Water_Sea_" + variant + ".png";
            Texture2D texture = BuildWaterTexture(variant);
            File.WriteAllBytes(pngPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16f;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (sprite == null)
            {
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(pngPath))
                {
                    if (asset is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            if (sprite == null)
            {
                Debug.LogWarning($"[Terrain] No se pudo importar el sprite de agua '{pngPath}'.");
                return null;
            }

            string tilePath = tilesFolder + "/Water_Sea_" + variant + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, tilePath);
            }

            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Texture2D BuildWaterTexture(int variant)
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color deep = new Color(0.04f, 0.20f, 0.34f, 1f);
            Color mid = new Color(0.07f, 0.34f, 0.50f, 1f);
            Color light = new Color(0.14f, 0.53f, 0.68f, 1f);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float wave = Mathf.Sin((x + variant * 4.3f) * 0.8f + Mathf.Sin((y + variant * 1.7f) * 0.55f) * 1.5f);
                    float ripple = Mathf.Sin(x * 0.45f - y * 0.7f + variant * 2.1f);

                    Color color = deep;
                    if (wave > 0.65f || ripple > 0.9f)
                    {
                        color = light;
                    }
                    else if (wave > 0.1f)
                    {
                        color = mid;
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private static void SetCatalogWater(TilesetCatalogSO catalog, List<TileBase> waterTiles)
        {
            TerrainTileLayer layer = null;
            for (int i = 0; i < catalog.TerrainTileLayers.Count; i++)
            {
                if (catalog.TerrainTileLayers[i] != null && catalog.TerrainTileLayers[i].type == TerrainType.Water)
                {
                    layer = catalog.TerrainTileLayers[i];
                    break;
                }
            }

            if (layer == null)
            {
                layer = new TerrainTileLayer { type = TerrainType.Water };
                catalog.TerrainTileLayers.Add(layer);
            }

            if (layer.tiles == null)
            {
                layer.tiles = new List<TileBase>();
            }

            layer.tiles.Clear();
            layer.tiles.AddRange(waterTiles);
            EditorUtility.SetDirty(catalog);
        }

        private static T FindAsset<T>(string name) where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null && asset.name == name)
                {
                    return asset;
                }
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent)) parent = parent.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
