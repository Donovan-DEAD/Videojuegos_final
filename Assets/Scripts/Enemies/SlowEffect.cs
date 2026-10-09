using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Aplica una ralentización temporal al EnemyMovement y restaura su velocidad al expirar.
    /// </summary>
    public class SlowEffect : MonoBehaviour
    {
        [SerializeField] private EnemyMovement movement;

        private float timer;
        private float multiplier = 1f;

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<EnemyMovement>();
            }
        }

        /// <summary>Aplica una ralentización con multiplicador (limitado a [0.05, 1]) y duración dados.</summary>
        public void Apply(float slowMultiplier, float duration)
        {
            multiplier = Mathf.Clamp(slowMultiplier, 0.05f, 1f);
            timer = duration;

            if (movement != null)
            {
                movement.SpeedMultiplier = multiplier;
            }
        }

        /// <summary>Descuenta la duración y restaura la velocidad al terminar.</summary>
        private void Update()
        {
            if (timer <= 0f)
            {
                return;
            }

            timer -= Time.deltaTime;

            if (timer <= 0f && movement != null)
            {
                movement.SpeedMultiplier = 1f;
            }
        }
    }
}
