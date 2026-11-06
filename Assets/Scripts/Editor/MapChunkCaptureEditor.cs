using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Ventana de editor que captura un cuarto (Tilemap) a un MapChunkSO (tiles, tamaño en celdas,
    /// tag y puntos de spawn tomados de hijos con prefijo) y valida terreno, aperturas y spawns.
    /// </summary>
    public class MapChunkCaptureEditor : EditorWindow
    {
        private Tilemap source;
        private MapChunkSO target;
        private TilesetCatalogSO catalog;
        private int cellSize = 16;
        private RoomTag tag = RoomTag.None;
        private CellRole role = CellRole.Normal;
        private bool captureSpawnPoints = true;
        private string spawnPrefix = "EnemySpawn";
        private bool autoGenerateSpawns = true;
        private int spawnGrid = 3;

        /// <summary>
        /// Abre la ventana de captura de cuartos del editor.
        /// </summary>
        [MenuItem("Tools/Madness Dreams/Map Chunk Capture")]
        public static void Open()
        {
            GetWindow<MapChunkCaptureEditor>("Map Chunk Capture");
        }

        /// <summary>
        /// Menú de editor que comprueba que cada WorldConfigSO genere exactamente una sala Start y una Boss.
        /// </summary>
        [MenuItem("Tools/Madness Dreams/Validate Worlds")]
        public static void ValidateWorlds()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:WorldConfigSO"))
            {
                var config = AssetDatabase.LoadAssetAtPath<WorldConfigSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (config == null)
                {
                    continue;
                }

                var rooms = new List<RoomPlacement>();
                var connections = new List<RoomConnection>();
                RoomLayout.Build(config, new System.Random(config.Seed), rooms, connections);

                int starts = 0;
                int bosses = 0;
                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].role == CellRole.Start) starts++;
                    if (rooms[i].role == CellRole.Boss) bosses++;
                }

                bool ok = rooms.Count > 0 && starts == 1 && bosses == 1;
                Debug.Log($"[Worlds] {config.name}: cuartos={rooms.Count} start={starts} boss={bosses} -> {(ok ? "OK" : "REVISAR")}", config);
            }
        }

        private void OnGUI()
        {
            source = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", source, typeof(Tilemap), true);
            target = (MapChunkSO)EditorGUILayout.ObjectField("Target MapChunkSO", target, typeof(MapChunkSO), false);
            catalog = (TilesetCatalogSO)EditorGUILayout.ObjectField("Tileset Catalog", catalog, typeof(TilesetCatalogSO), false);
            cellSize = Mathf.Max(1, EditorGUILayout.IntField("Cell size (tiles)", cellSize));
            tag = (RoomTag)EditorGUILayout.EnumPopup("Tag", tag);
            role = (CellRole)EditorGUILayout.EnumPopup("Role", role);
            captureSpawnPoints = EditorGUILayout.Toggle("Capture spawn points", captureSpawnPoints);

            using (new EditorGUI.DisabledScope(!captureSpawnPoints))
            {
                spawnPrefix = EditorGUILayout.TextField("Spawn name prefix", spawnPrefix);
            }

            autoGenerateSpawns = EditorGUILayout.Toggle("Generar spawns si quedan 0", autoGenerateSpawns);
            using (new EditorGUI.DisabledScope(!autoGenerateSpawns))
            {
                spawnGrid = Mathf.Clamp(EditorGUILayout.IntField("Grilla de respaldo (NxN)", spawnGrid), 1, 8);
            }

            bool canCapture = source != null && target != null;
            using (new EditorGUI.DisabledScope(!canCapture))
            {
                if (GUILayout.Button("Capture"))
                {
                    Capture(source, target, catalog, cellSize, tag, role, captureSpawnPoints, spawnPrefix, autoGenerateSpawns, spawnGrid);
                }
            }

            using (new EditorGUI.DisabledScope(target == null))
            {
                if (GUILayout.Button("Preview"))
                {
                    RoomPreviewRenderer.RenderRoom(target, "Assets/Previews/MapChunks/" + target.name + ".png", 8);
                }
            }
        }

        /// <summary>
        /// Captura el Tilemap origen en el MapChunkSO destino y valida terreno/aperturas/spawns.
        /// </summary>
        public static void Capture(
            Tilemap source,
            MapChunkSO target,
            TilesetCatalogSO catalog,
            int cellSize,
            RoomTag tag,
            CellRole role,
            bool captureSpawnPoints,
            string spawnPrefix,
            bool autoGenerateSpawns,
            int spawnGrid)
        {
            if (source == null || target == null)
            {
                return;
            }

            BoundsInt bounds = ComputeTileBounds(source);
            Vector2Int size = new Vector2Int(bounds.size.x, bounds.size.y);
            TileBase[] flattened = source.GetTilesBlock(bounds);
            Vector2Int cells = new Vector2Int(
                Mathf.Max(1, Mathf.RoundToInt(size.x / (float)cellSize)),
                Mathf.Max(1, Mathf.RoundToInt(size.y / (float)cellSize)));

            target.SetSize(size);
            target.SetTileData(flattened);
            target.SetSizeInCells(cells);
            target.SetTag(tag);
            var roleSo = new SerializedObject(target);
            var roleProp = roleSo.FindProperty("role");
            if (roleProp != null)
            {
                roleProp.intValue = (int)role;
                roleSo.ApplyModifiedPropertiesWithoutUndo();
            }

            int spawnCount = 0;
            if (captureSpawnPoints)
            {
                List<EnemySpawnPoint> found = GatherSpawnPoints(source, bounds, spawnPrefix);
                spawnCount = found.Count;

                var so = new SerializedObject(target);
                var spawns = so.FindProperty("enemySpawns");
                spawns.arraySize = found.Count;
                for (int i = 0; i < found.Count; i++)
                {
                    var element = spawns.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("localPosition").vector2IntValue = found[i].localPosition;
                    element.FindPropertyRelative("weight").intValue = found[i].weight;
                    element.FindPropertyRelative("terrain").intValue = (int)found[i].terrain;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Respaldo: si el bloque queda sin spawn points y su rol es Normal, se genera una grilla.
            if (autoGenerateSpawns && target.Role != CellRole.Start && target.Role != CellRole.Boss)
            {
                EnemySpawnPoint[] current = target.EnemySpawns;
                if (current == null || current.Length == 0)
                {
                    target.SetEnemySpawns(TilesetTools.BuildSpawnGrid(size, spawnGrid));
                    spawnCount = target.EnemySpawns.Length;
                    Debug.Log($"'{target.name}' no tenía spawn points: se generó una grilla {spawnGrid}x{spawnGrid}.", target);
                }
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            TerrainTools.ValidateChunk(target, catalog);

            if (captureSpawnPoints && spawnCount == 0)
            {
                Debug.LogWarning($"El cuarto '{target.name}' no encontró spawn points con el prefijo '{spawnPrefix}'.", target);
            }

            Debug.Log($"Captured {size.x}x{size.y} tiles ({cells.x}x{cells.y} celdas, tag={tag}) into {target.name}.", target);
        }

        private static BoundsInt ComputeTileBounds(Tilemap source)
        {
            BoundsInt probe = source.cellBounds;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

            foreach (Vector3Int pos in probe.allPositionsWithin)
            {
                if (!source.HasTile(pos))
                {
                    continue;
                }

                if (pos.x < minX) minX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y > maxY) maxY = pos.y;
            }

            if (minX == int.MaxValue)
            {
                return new BoundsInt(0, 0, 0, 0, 0, 1);
            }

            return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
        }

        private static List<EnemySpawnPoint> GatherSpawnPoints(Tilemap source, BoundsInt bounds, string spawnPrefix)
        {
            var result = new List<EnemySpawnPoint>();

            foreach (Transform candidate in GatherCandidates(source))
            {
                if (candidate == null || string.IsNullOrEmpty(spawnPrefix) || !candidate.name.StartsWith(spawnPrefix))
                {
                    continue;
                }

                Vector3Int cell = source.WorldToCell(candidate.position);
                bool water = candidate.name.IndexOf("Water", System.StringComparison.OrdinalIgnoreCase) >= 0;
                result.Add(new EnemySpawnPoint
                {
                    localPosition = new Vector2Int(cell.x - bounds.x, cell.y - bounds.y),
                    weight = 1,
                    terrain = water ? TerrainType.Water : TerrainType.Void
                });
            }

            return result;
        }

        private static IEnumerable<Transform> GatherCandidates(Tilemap source)
        {
            Transform parent = source.transform.parent;
            if (parent != null)
            {
                foreach (Transform child in parent)
                {
                    yield return child;
                }
            }

            foreach (Transform child in source.transform)
            {
                yield return child;
            }
        }
    }
}
