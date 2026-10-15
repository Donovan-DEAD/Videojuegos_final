using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Herramientas de editor para validar y reparar los <see cref="MapChunkSO"/>: tamaño múltiplo de la
    /// celda, anillo de muro completo y ausencia de huecos que dejen salir del mapa.
    /// </summary>
    public static class MapChunkTools
    {
        private const int CellSize = 16;

        /// <summary>
        /// Menú que revisa todos los <see cref="MapChunkSO"/>: tamaño no múltiplo de la celda, anillo de
        /// muro incompleto y celdas vacías. Registra un resumen en consola.
        /// </summary>
        [MenuItem("Tools/Madness Dreams/Validate Chunks")]
        public static void ValidateChunks()
        {
            StringBuilder report = new StringBuilder();
            int problems = 0;

            foreach (MapChunkSO chunk in LoadAllChunks())
            {
                Vector2Int size = chunk.Size;
                TileBase[] tiles = chunk.Tiles;

                if (size.x <= 0 || size.y <= 0)
                {
                    report.AppendLine($"- {chunk.name}: tamaño inválido {size}.");
                    problems++;
                    continue;
                }

                if (size.x % CellSize != 0 || size.y % CellSize != 0)
                {
                    report.AppendLine($"- {chunk.name}: tamaño {size} no es múltiplo de {CellSize} (costura abierta).");
                    problems++;
                }

                if (tiles == null || tiles.Length != size.x * size.y)
                {
                    report.AppendLine($"- {chunk.name}: tiles {tiles?.Length ?? 0} != {size.x * size.y}.");
                    problems++;
                    continue;
                }

                int ringNulls = CountRingNulls(tiles, size);
                if (ringNulls > 0)
                {
                    report.AppendLine($"- {chunk.name}: {ringNulls} celdas vacías en el anillo de muro.");
                    problems++;
                }
            }

            if (problems == 0)
            {
                Debug.Log("[Chunks] Todos los MapChunkSO son válidos (tamaño múltiplo de la celda y anillo completo).");
            }
            else
            {
                Debug.LogWarning($"[Chunks] {problems} problema(s):\n{report}");
            }
        }

        /// <summary>
        /// Menú que normaliza cada <see cref="MapChunkSO"/>: sube el tamaño al siguiente múltiplo de la
        /// celda (rellenando con muro), tapa las celdas vacías del anillo y reconstruye como cuarto limpio
        /// los que están mayormente vacíos.
        /// </summary>
        [MenuItem("Tools/Madness Dreams/Repair Chunks")]
        public static void RepairChunks()
        {
            TilesetCatalogSO catalog = FindCatalog();
            if (catalog == null)
            {
                Debug.LogError("[Chunks] No hay ningún TilesetCatalogSO con piso y muro: no se puede reparar.");
                return;
            }

            TileBase wall = catalog.WallTiles[0];
            TileBase floor = catalog.FloorTiles[0];

            int repaired = 0;

            foreach (MapChunkSO chunk in LoadAllChunks())
            {
                Vector2Int size = chunk.Size;
                if (size.x <= 0 || size.y <= 0)
                {
                    continue;
                }

                int targetW = Mathf.CeilToInt(size.x / (float)CellSize) * CellSize;
                int targetH = Mathf.CeilToInt(size.y / (float)CellSize) * CellSize;

                TileBase[] tiles = chunk.Tiles ?? new TileBase[0];
                bool changed = size.x != targetW || size.y != targetH;

                if (tiles.Length != size.x * size.y && tiles.Length != targetW * targetH)
                {
                    // Datos corruptos: se reconstruye como cuarto limpio.
                    tiles = BuildRoom(targetW, targetH, wall, floor);
                    changed = true;
                }
                else if (size.x != targetW || size.y != targetH)
                {
                    tiles = Resize(tiles, size, new Vector2Int(targetW, targetH), wall);
                    changed = true;
                }
                else if (IsMostlyEmpty(tiles, new Vector2Int(targetW, targetH)))
                {
                    tiles = BuildRoom(targetW, targetH, wall, floor);
                    changed = true;
                }
                else if (FillRingNulls(tiles, new Vector2Int(targetW, targetH), wall) > 0)
                {
                    changed = true;
                }

                if (!changed)
                {
                    continue;
                }

                chunk.SetSize(new Vector2Int(targetW, targetH));
                chunk.SetTileData(tiles);
                EditorUtility.SetDirty(chunk);
                repaired++;
                Debug.Log($"[Chunks] Reparado '{chunk.name}' -> {targetW}x{targetH}.", chunk);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Chunks] Reparación completa: {repaired} cuarto(s) modificado(s).");
        }

        private static IEnumerable<MapChunkSO> LoadAllChunks()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:MapChunkSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MapChunkSO chunk = AssetDatabase.LoadAssetAtPath<MapChunkSO>(path);
                if (chunk != null)
                {
                    yield return chunk;
                }
            }
        }

        private static TilesetCatalogSO FindCatalog()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:TilesetCatalogSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TilesetCatalogSO catalog = AssetDatabase.LoadAssetAtPath<TilesetCatalogSO>(path);
                if (catalog != null && catalog.WallTiles.Count > 0 && catalog.FloorTiles.Count > 0)
                {
                    return catalog;
                }
            }

            return null;
        }

        private static bool IsRing(int x, int y, Vector2Int size)
        {
            return x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1;
        }

        private static int CountRingNulls(TileBase[] tiles, Vector2Int size)
        {
            int count = 0;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (IsRing(x, y, size) && tiles[y * size.x + x] == null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static int FillRingNulls(TileBase[] tiles, Vector2Int size, TileBase wall)
        {
            int filled = 0;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int index = y * size.x + x;
                    if (IsRing(x, y, size) && tiles[index] == null)
                    {
                        tiles[index] = wall;
                        filled++;
                    }
                }
            }

            return filled;
        }

        private static bool IsMostlyEmpty(TileBase[] tiles, Vector2Int size)
        {
            int total = size.x * size.y;
            int empty = 0;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == null)
                {
                    empty++;
                }
            }

            return total > 0 && empty * 2 > total;
        }

        private static TileBase[] Resize(TileBase[] tiles, Vector2Int from, Vector2Int to, TileBase wall)
        {
            TileBase[] result = new TileBase[to.x * to.y];
            for (int y = 0; y < to.y; y++)
            {
                for (int x = 0; x < to.x; x++)
                {
                    if (x < from.x && y < from.y)
                    {
                        int source = y * from.x + x;
                        result[y * to.x + x] = source < tiles.Length ? tiles[source] : null;
                    }
                    else
                    {
                        result[y * to.x + x] = wall;
                    }
                }
            }

            return result;
        }

        private static TileBase[] BuildRoom(int w, int h, TileBase wall, TileBase floor)
        {
            TileBase[] tiles = new TileBase[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tiles[y * w + x] = (x == 0 || y == 0 || x == w - 1 || y == h - 1) ? wall : floor;
                }
            }

            return tiles;
        }
    }
}
