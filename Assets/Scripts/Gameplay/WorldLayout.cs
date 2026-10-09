using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Coloca al jugador y la arena del jefe en los centros de las salas de inicio y jefe cuando MapGenerator genera el mundo.
    /// </summary>
    public class WorldLayout : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private Transform player;
        [SerializeField] private Transform bossArena;
        [SerializeField] private Tilemap tilemap;

        /// <summary>
        /// Se suscribe a la generación del mapa y aplica la disposición de inmediato.
        /// </summary>
        private void OnEnable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Generated += OnGenerated;
            }

            OnGenerated();
        }

        /// <summary>
        /// Deja de escuchar la generación del mapa.
        /// </summary>
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
                player.position = target.GetCellCenterWorld(mapGenerator.StartRoomCenterCell);
            }

            if (bossArena != null)
            {
                bossArena.position = target.GetCellCenterWorld(mapGenerator.BossRoomCenterCell);
            }
        }
    }
}
