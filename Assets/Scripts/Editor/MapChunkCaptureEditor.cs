using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Ventana de editor que captura un cuarto (Tilemap) a un MapChunkSO (tiles, tamaño en celdas, tag y puntos de spawn tomados de
    /// hijos con prefijo) y avisa de problemas de tamaño, anillo de muro o spawns. Incluye el menú "Validate Worlds".
    /// </summary>
    public class MapChunkCaptureEditor : EditorWindow
    {
        private Tilemap source;
        private MapChunkSO target;
        private int cellSize = 16;
        private RoomTag tag = RoomTag.None;
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
        /// Menú de editor que comprueba que cada WorldConfigSO genere exactamente una sala Start y una Boss, registrando el resultado.
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
            cellSize = Mathf.Max(1, EditorGUILayout.IntField("Cell size (tiles)", cellSize));
            tag = (RoomTag)EditorGUILayout.EnumPopup("Tag", tag);
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
                    Capture(source, target, cellSize, tag, captureSpawnPoints, spawnPrefix, autoGenerateSpawns, spawnGrid);
                }
            }
        }

        /// <summary>
        /// Captura el Tilemap origen en el MapChunkSO destino (tiles, tamaño en celdas, tag y puntos de spawn) y lanza las validaciones.
        /// </summary>
        public static void Capture(Tilemap source, MapChunkSO target, int cellSize, RoomTag tag, bool captureSpawnPoints, string spawnPrefix, bool autoGenerateSpawns, int spawnGrid)
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

            int spawnCount = 0;
            if (captureSpawnPoints)
            {
                List<Vector2Int> found = GatherSpawnPoints(source, bounds, spawnPrefix);
                spawnCount = found.Count;

                var so = new SerializedObject(target);
                var spawns = so.FindProperty("enemySpawns");
                spawns.arraySize = found.Count;
                for (int i = 0; i < found.Count; i++)
                {
                    var element = spawns.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("localPosition").vector2IntValue = found[i];
                    element.FindPropertyRelative("weight").intValue = 1;
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

            Validate(target, size, flattened, cellSize, captureSpawnPoints, spawnCount, spawnPrefix);

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

        private static List<Vector2Int> GatherSpawnPoints(Tilemap source, BoundsInt bounds, string spawnPrefix)
        {
            var result = new List<Vector2Int>();

            foreach (Transform candidate in GatherCandidates(source))
            {
                if (candidate == null || string.IsNullOrEmpty(spawnPrefix) || !candidate.name.StartsWith(spawnPrefix))
                {
                    continue;
                }

                Vector3Int cell = source.WorldToCell(candidate.position);
                result.Add(new Vector2Int(cell.x - bounds.x, cell.y - bounds.y));
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

        private static void Validate(MapChunkSO target, Vector2Int size, TileBase[] tiles, int cellSize, bool captureSpawnPoints, int spawnCount, string spawnPrefix)
        {
            if (size.x % cellSize != 0 || size.y % cellSize != 0)
            {
                Debug.LogError($"El cuarto '{target.name}' mide {size} y no es múltiplo de la celda ({cellSize}): dejará una costura abierta al estamparse.", target);
            }

            if (tiles != null && tiles.Length == size.x * size.y && size.x > 1 && size.y > 1)
            {
                bool ringOk = true;
                for (int x = 0; x < size.x && ringOk; x++)
                {
                    if (tiles[x] == null || tiles[(size.y - 1) * size.x + x] == null)
                    {
                        ringOk = false;
                    }
                }

                for (int y = 0; y < size.y && ringOk; y++)
                {
                    if (tiles[y * size.x] == null || tiles[y * size.x + size.x - 1] == null)
                    {
                        ringOk = false;
                    }
                }

                if (!ringOk)
                {
                    Debug.LogWarning($"El cuarto '{target.name}' no tiene el anillo de muro de 1 tile en el borde.", target);
                }
            }

            if (captureSpawnPoints && spawnCount == 0)
            {
                Debug.LogWarning($"El cuarto '{target.name}' no encontró spawn points con el prefijo '{spawnPrefix}'.", target);
            }
        }
    }
}
