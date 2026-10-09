using UnityEngine;

namespace MadnessDreams
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;

        private int damage;
        private LayerMask targetMask;
        private float slowMultiplier = 1f;
        private float slowDuration;

        private void Awake()
        {
            Destroy(gameObject, lifetime);
        }

        public void Launch(Vector2 direction, int damage, float speed, LayerMask targetMask, float slowMultiplier = 1f, float slowDuration = 0f)
        {
            this.damage = damage;
            this.targetMask = targetMask;
            this.slowMultiplier = slowMultiplier;
            this.slowDuration = slowDuration;

            Vector2 dir = direction.normalized;
            transform.up = dir;
            GetComponent<Rigidbody2D>().linearVelocity = dir * speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                ApplySlow(other);
            }

            Destroy(gameObject);
        }

        private void ApplySlow(Collider2D other)
        {
            if (slowDuration <= 0f)
            {
                return;
            }

            EnemyMovement movement = other.GetComponentInParent<EnemyMovement>();
            if (movement == null)
            {
                return;
            }

            SlowEffect slow = movement.GetComponent<SlowEffect>();
            if (slow == null)
            {
                slow = movement.gameObject.AddComponent<SlowEffect>();
            }

            slow.Apply(slowMultiplier, slowDuration);
        }
    }
}
