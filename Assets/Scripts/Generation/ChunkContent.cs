using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    public class ChunkContent : MonoBehaviour
    {
        [SerializeField] private WorldConfigSO config;
        [SerializeField] private Spawner spawner;
        [SerializeField] private Tilemap tilemap;

        public void Configure(WorldConfigSO worldConfig)
        {
            config = worldConfig;
        }

        public void SpawnEnemies(ChunkPlacement placement, System.Random rng)
        {
            if (config == null || spawner == null || tilemap == null)
            {
                return;
            }

            System.Collections.Generic.List<EnemyArchetypeSO> pool = config.EnemyPool;
            if (pool == null || pool.Count == 0)
            {
                return;
            }

            MapChunkSO chunk = placement.chunk;
            if (chunk == null)
            {
                return;
            }

            EnemySpawnPoint[] points = chunk.EnemySpawns;
            if (points == null || points.Length == 0)
            {
                return;
            }

            int toSpawn = Mathf.Min(config.EnemiesPerChunk, points.Length);

            for (int i = 0; i < toSpawn; i++)
            {
                EnemySpawnPoint point = points[i];
                Vector3Int cell = placement.originCell + new Vector3Int(point.localPosition.x, point.localPosition.y, 0);
                Vector3 world = tilemap.GetCellCenterWorld(cell);

                EnemyArchetypeSO archetype = pool[rng.Next(pool.Count)];
                spawner.TrySpawn(archetype.Prefab, world, rng, out _);
            }
        }
    }
}
