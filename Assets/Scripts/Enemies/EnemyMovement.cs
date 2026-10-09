using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Mueve al enemigo aplicando velocidades a su Rigidbody2D, con un multiplicador global de velocidad
    /// (modificable p. ej. por SlowEffect).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        private Rigidbody2D rb;

        /// <summary>Factor aplicado a todos los desplazamientos del enemigo.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>Se mueve en la dirección dada a la velocidad indicada.</summary>
        public void MoveDirection(Vector2 direction, float speed)
        {
            rb.linearVelocity = direction.normalized * speed * SpeedMultiplier;
        }

        /// <summary>Avanza hacia un punto, deteniéndose si ya está sobre él.</summary>
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

        /// <summary>Fija directamente la velocidad del Rigidbody2D.</summary>
        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
        }

        /// <summary>Detiene el movimiento.</summary>
        public void Stop()
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
