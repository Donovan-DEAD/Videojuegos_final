using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    public class WorldLayout : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private Transform player;
        [SerializeField] private Transform bossArena;
        [SerializeField] private Tilemap tilemap;

        private void OnEnable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated += OnGenerated;
            }

            OnGenerated();
        }

        private void OnDisable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated -= OnGenerated;
            }
        }

        private void OnGenerated()
        {
            if (mapGenerator == null || mapGenerator.Config == null)
            {
                return;
            }

            Tilemap target = tilemap != null ? tilemap : mapGenerator.TargetTilemap;
            if (target == null)
            {
                return;
            }

            if (player != null)
            {
                player.position = target.GetCellCenterWorld(mapGenerator.ChunkCenterCell(mapGenerator.StartChunk));
            }

            if (bossArena != null)
            {
                bossArena.position = target.GetCellCenterWorld(mapGenerator.ChunkCenterCell(mapGenerator.BossOriginChunk));
            }
        }
    }
}
