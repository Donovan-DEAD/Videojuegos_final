using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Fragmento de mapa (sala) definido por su tamaño, etiqueta, rol, tiles y puntos de
    /// aparición de enemigos y regiones de props.
    /// </summary>
    [CreateAssetMenu(fileName = "MapChunk", menuName = "Madness Dreams/Map Chunk")]
    public class MapChunkSO : ScriptableObject
    {
        [Header("Layout")]
        [SerializeField] private Vector2Int sizeInCells = new Vector2Int(1, 1);
        [SerializeField] private RoomTag tag = RoomTag.None;
        [SerializeField] private CellRole role = CellRole.Normal;

        [Header("Size (in tiles)")]
        [SerializeField] private Vector2Int size = new Vector2Int(16, 16);

        [Header("Tiles (flattened, row-major)")]
        [SerializeField] private TileBase[] tiles;

        [Header("Enemy spawn points (local tile positions)")]
        [SerializeField] private EnemySpawnPoint[] enemySpawns;

        [Header("Prop regions (local tile rects)")]
        [SerializeField] private PropRegion[] propRegions;

        public Vector2Int SizeInCells => sizeInCells;
        public RoomTag Tag => tag;
        public CellRole Role => role;
        public Vector2Int Size => size;
        public TileBase[] Tiles => tiles;
        public EnemySpawnPoint[] EnemySpawns => enemySpawns;
        public PropRegion[] PropRegions => propRegions;

        public int TileCount => size.x * size.y;

        public BoundsInt LocalArea => new BoundsInt(0, 0, 0, size.x, size.y, 1);

        /// <summary>
        /// Devuelve el índice lineal del tile en la matriz aplanada en orden por filas (row-major).
        /// </summary>
        public int TileIndex(int x, int y)
        {
            return y * size.x + x;
        }

        /// <summary>
        /// Devuelve el tile en las coordenadas indicadas, o null si quedan fuera de los límites.
        /// </summary>
        public TileBase GetTile(int x, int y)
        {
            if (x < 0 || x >= size.x || y < 0 || y >= size.y)
            {
                return null;
            }

            if (tiles == null || tiles.Length == 0)
            {
                return null;
            }

            int index = TileIndex(x, y);
            if (index < 0 || index >= tiles.Length)
            {
                return null;
            }

            return tiles[index];
        }

        /// <summary>
        /// Calcula el tamaño de la sala en celdas a partir del tamaño en tiles y el tamaño de
        /// celda; usa <c>sizeInCells</c> si no hay un tamaño en tiles válido.
        /// </summary>
        public Vector2Int ResolveSizeInCells(int cellSize)
        {
            if (cellSize <= 0)
            {
                return sizeInCells.x > 0 && sizeInCells.y > 0 ? sizeInCells : Vector2Int.one;
            }

            if (size.x > 0 && size.y > 0)
            {
                return new Vector2Int(
                    Mathf.Max(1, Mathf.RoundToInt(size.x / (float)cellSize)),
                    Mathf.Max(1, Mathf.RoundToInt(size.y / (float)cellSize)));
            }

            return sizeInCells.x > 0 && sizeInCells.y > 0 ? sizeInCells : Vector2Int.one;
        }

        public void SetSize(Vector2Int newSize)
        {
            size = newSize;
        }

        public void SetTileData(TileBase[] newTiles)
        {
            tiles = newTiles;
        }

        public void SetSizeInCells(Vector2Int value)
        {
            sizeInCells = value;
        }

        public void SetTag(RoomTag value)
        {
            tag = value;
        }
    }

    /// <summary>
    /// Punto de aparición de enemigos dentro de un fragmento, con su peso relativo.
    /// </summary>
    [System.Serializable]
    public struct EnemySpawnPoint
    {
        public Vector2Int localPosition;
        public int weight;
    }

    /// <summary>
    /// Región rectangular local, en tiles, donde pueden colocarse props.
    /// </summary>
    [System.Serializable]
    public struct PropRegion
    {
        public Vector2Int localMin;
        public Vector2Int size;
    }
}
