using UnityEngine;

namespace MadnessDreams
{
    public class EnemyFOV : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private Transform eyes;

        private EnemyStatsSO stats;

        public void SetStats(EnemyStatsSO value)
        {
            stats = value;
        }

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
