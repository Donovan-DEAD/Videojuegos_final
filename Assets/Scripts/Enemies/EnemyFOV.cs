using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Percepción del enemigo: visión por radio y cono con línea de visión, y audición por radio,
    /// según EnemyStatsSO.
    /// </summary>
    public class EnemyFOV : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private Transform eyes;

        private EnemyStatsSO stats;
        private int baseObstacleMask;
        private bool baseMaskCached;

        /// <summary>Asigna las estadísticas que definen visión y audición.</summary>
        public void SetStats(EnemyStatsSO value)
        {
            if (!baseMaskCached)
            {
                baseObstacleMask = obstacleMask.value;
                baseMaskCached = true;
            }

            stats = value;

            int mask = baseObstacleMask;
            if (value != null && value.VisionThroughWalls)
            {
                mask &= ~LayerMask.GetMask("Walls");
            }

            LayerMask layerMask = obstacleMask;
            layerMask.value = mask;
            obstacleMask = layerMask;
        }

        /// <summary>Indica si el jugador está dentro del radio de audición.</summary>
        public bool CanHearPlayer(Transform player)
        {
            if (stats == null || player == null)
            {
                return false;
            }

            Vector3 origin = eyes != null ? eyes.position : transform.position;
            float sqrDistance = (player.position - origin).sqrMagnitude;
            return sqrDistance <= stats.HearingRadius * stats.HearingRadius;
        }

        /// <summary>Indica si el jugador está en el cono de visión y con línea de visión despejada.</summary>
        public bool CanSeePlayer(Transform player)
        {
            if (stats == null || player == null)
            {
                return false;
            }

            Vector3 origin = eyes != null ? eyes.position : transform.position;
            Vector2 toPlayer = (Vector2)(player.position - origin);
            float sqrDistance = toPlayer.sqrMagnitude;

            if (sqrDistance > stats.FovRadius * stats.FovRadius)
            {
                return false;
            }

            if (stats.FovAngle < 360f)
            {
                Vector2 forward = (Vector2)transform.up;
                float angle = Vector2.Angle(forward, toPlayer);
                if (angle > stats.FovAngle * 0.5f)
                {
                    return false;
                }
            }

            return HasLineOfSight(origin, player.position);
        }

        private bool HasLineOfSight(Vector3 origin, Vector3 target)
        {
            Vector2 direction = (Vector2)(target - origin);
            float distance = direction.magnitude;

            if (distance <= 0.0001f)
            {
                return true;
            }

            RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, distance, obstacleMask);
            return hit.collider == null;
        }
    }
}
