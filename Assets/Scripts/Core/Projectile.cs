using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Proyectil que se autodestruye tras un tiempo, hace daño a los objetivos de su máscara
    /// y aplica una ralentización opcional al impactar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;

        private int damage;
        private LayerMask targetMask;
        private float slowMultiplier = 1f;
        private float slowDuration;

        /// <summary>
        /// Programa la destrucción automática del proyectil tras su tiempo de vida.
        /// </summary>
        private void Awake()
        {
            Destroy(gameObject, lifetime);
        }

        /// <summary>
        /// Lanza el proyectil en una dirección con daño, velocidad, máscara de objetivos y,
        /// opcionalmente, un multiplicador y duración de ralentización.
        /// </summary>
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
