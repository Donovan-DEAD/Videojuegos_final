using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    [CreateAssetMenu(fileName = "MapChunk", menuName = "Madness Dreams/Map Chunk")]
    public class MapChunkSO : ScriptableObject
    {
        [Header("Size (in tiles)")]
        [SerializeField] private Vector2Int size = new Vector2Int(16, 16);

        [Header("Tiles (flattened, row-major)")]
        [SerializeField] private TileBase[] tiles;

        [Header("Doors (local tile positions)")]
        [SerializeField] private Vector2Int[] doorPositions;

        [Header("Enemy spawn points (local tile positions)")]
        [SerializeField] private EnemySpawnPoint[] enemySpawns;

        [Header("Role")]
        [SerializeField] private CellRole role = CellRole.Normal;

        public Vector2Int Size => size;
        public TileBase[] Tiles => tiles;
        public Vector2Int[] DoorPositions => doorPositions;
        public EnemySpawnPoint[] EnemySpawns => enemySpawns;
        public CellRole Role => role;

        public int TileCount => size.x * size.y;

        public BoundsInt LocalArea => new BoundsInt(0, 0, 0, size.x, size.y, 1);

        public int TileIndex(int x, int y)
        {
            return y * size.x + x;
        }

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

        public void SetSize(Vector2Int newSize)
        {
            size = newSize;
        }

        public void SetTileData(TileBase[] newTiles)
        {
            tiles = newTiles;
        }
    }

    [System.Serializable]
    public struct EnemySpawnPoint
    {
        public Vector2Int localPosition;
        public int weight;
    }
}
