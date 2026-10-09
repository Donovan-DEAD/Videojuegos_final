using UnityEngine;

namespace MadnessDreams
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        private Rigidbody2D rb;

        public float SpeedMultiplier { get; set; } = 1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void MoveDirection(Vector2 direction, float speed)
        {
            rb.linearVelocity = direction.normalized * speed * SpeedMultiplier;
        }

        public void MoveTowards(Vector3 target, float speed)
        {
            Vector2 direction = (Vector2)(target - transform.position);
            if (direction.sqrMagnitude < 0.0001f)
            {
                Stop();
                return;
            }

            rb.linearVelocity = direction.normalized * speed * SpeedMultiplier;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
        }

        public void Stop()
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
