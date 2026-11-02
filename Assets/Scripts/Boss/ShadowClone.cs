using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Clon de sombra de Peter Pan: persigue al jugador, hace daño por contacto y se desvanece.</summary>
    public class ShadowClone : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private int damage = 6;
        [SerializeField] private float damageInterval = 0.8f;

        private Transform target;
        private Rigidbody2D rb;
        private float life = 4f;
        private float damageTimer;

        public void Setup(Transform target, Vector3 origin, float duration)
        {
            this.target = target;
            life = duration;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }

            if (damageTimer > 0f) damageTimer -= Time.deltaTime;

            if (target == null || rb == null) return;

            Vector2 to = (Vector2)(target.position - transform.position);
            rb.linearVelocity = to.sqrMagnitude > 0.0001f ? to.normalized * moveSpeed : Vector2.zero;

            if (to.magnitude <= 1.0f && damageTimer <= 0f)
            {
                Health health = target.GetComponent<Health>();
                if (health != null) health.TakeDamage(damage);
                damageTimer = damageInterval;
            }
        }
    }
}
