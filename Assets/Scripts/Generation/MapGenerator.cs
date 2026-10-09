using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WorldConfigSO config;
        [SerializeField] private Tilemap targetTilemap;

        [Header("Optional: enemy spawning")]
        [SerializeField] private ChunkContent chunkContent;

        public Vector2Int StartChunk { get; private set; }
        public Vector2Int BossOriginChunk { get; private set; }
        public Vector3Int BossOriginCell { get; private set; }
        public List<ChunkPlacement> NormalPlacements { get; } = new List<ChunkPlacement>();

        public WorldConfigSO Config => config;
        public Tilemap TargetTilemap => targetTilemap;
        public event System.Action Generated;

        public Vector3Int StartCell => ChunkToCell(StartChunk);

        public void Configure(WorldConfigSO worldConfig)
        {
            config = worldConfig;
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (config == null || config.Bank == null || targetTilemap == null)
            {
                Debug.LogError("MapGenerator: config, bank or targetTilemap is missing.", this);
                return;
            }

            targetTilemap.ClearAllTiles();
            NormalPlacements.Clear();

            System.Random rng = new System.Random(config.Seed);
            int width = Mathf.Max(1, config.Width);
            int height = Mathf.Max(1, config.Height);

            CellRole[,] grid = BuildRoles(width, height);
            StampNormalChunks(grid, width, height, rng);
            StampBossArena();

            if (chunkContent != null)
            {
                chunkContent.Configure(config);
                SpawnEnemies(rng);
            }

            Generated?.Invoke();
        }

        private CellRole[,] BuildRoles(int width, int height)
        {
            CellRole[,] grid = new CellRole[width, height];

            StartChunk = new Vector2Int(width / 2, height / 2);

            int region = Mathf.Clamp(config.BossRegionSize, 1, Mathf.Min(width, height));
            Vector2Int farthest = FindFarthestChunk(width, height, StartChunk);
            BossOriginChunk = new Vector2Int(
                Mathf.Clamp(farthest.x, 0, width - region),
                Mathf.Clamp(farthest.y, 0, height - region)
            );

            for (int cy = 0; cy < height; cy++)
            {
                for (int cx = 0; cx < width; cx++)
                {
                    bool inBoss = cx >= BossOriginChunk.x && cx < BossOriginChunk.x + region &&
                                  cy >= BossOriginChunk.y && cy < BossOriginChunk.y + region;
                    bool isStart = cx == StartChunk.x && cy == StartChunk.y;

                    if (inBoss)
                    {
                        grid[cx, cy] = CellRole.Boss;
                    }
                    else if (isStart)
                    {
                        grid[cx, cy] = CellRole.Start;
                    }
                    else
                    {
                        grid[cx, cy] = CellRole.Normal;
                    }
                }
            }

            return grid;
        }

        private void StampNormalChunks(CellRole[,] grid, int width, int height, System.Random rng)
        {
            BlockBankSO bank = config.Bank;

            for (int cy = 0; cy < height; cy++)
            {
                for (int cx = 0; cx < width; cx++)
                {
                    CellRole role = grid[cx, cy];
                    if (role != CellRole.Normal && role != CellRole.Start)
                    {
                        continue;
                    }

                    MapChunkSO chunk = bank.GetRandomChunk(rng);
                    if (chunk == null)
                    {
                        continue;
                    }

                    Vector3Int origin = ChunkToCell(new Vector2Int(cx, cy));
                    int rotation = rng.Next(4);
                    TilemapStamper.Stamp(targetTilemap, chunk, origin, rotation);

                    if (role == CellRole.Normal)
                    {
                        NormalPlacements.Add(new ChunkPlacement { chunk = chunk, originCell = origin });
                    }
                }
            }
        }

        private void StampBossArena()
        {
            MapChunkSO arena = config.Bank.BossArena;
            if (arena == null)
            {
                return;
            }

            BossOriginCell = ChunkToCell(BossOriginChunk);
            TilemapStamper.Stamp(targetTilemap, arena, BossOriginCell, 0);
        }

        private void SpawnEnemies(System.Random rng)
        {
            foreach (ChunkPlacement placement in NormalPlacements)
            {
                chunkContent.SpawnEnemies(placement, rng);
            }
        }

        public Vector3Int ChunkToCell(Vector2Int chunk)
        {
            return new Vector3Int(chunk.x * config.ChunkSize, chunk.y * config.ChunkSize, 0);
        }

        public Vector3Int ChunkCenterCell(Vector2Int chunk)
        {
            Vector3Int origin = ChunkToCell(chunk);
            int half = config != null ? Mathf.Max(0, config.ChunkSize / 2) : 0;
            return new Vector3Int(origin.x + half, origin.y + half, origin.z);
        }

        private static Vector2Int FindFarthestChunk(int width, int height, Vector2Int start)
        {
            Vector2Int farthest = start;
            int best = -1;

            for (int cy = 0; cy < height; cy++)
            {
                for (int cx = 0; cx < width; cx++)
                {
                    int distance = Mathf.Abs(cx - start.x) + Mathf.Abs(cy - start.y);
                    if (distance > best)
                    {
                        best = distance;
                        farthest = new Vector2Int(cx, cy);
                    }
                }
            }

            return farthest;
        }
    }

    [System.Serializable]
    public struct ChunkPlacement
    {
        public MapChunkSO chunk;
        public Vector3Int originCell;
    }
}
