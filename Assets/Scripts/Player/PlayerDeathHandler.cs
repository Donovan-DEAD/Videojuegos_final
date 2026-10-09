using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Escucha la muerte del jugador (Health.Died) y avisa a GameFlowManager.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        /// <summary>Se suscribe a Health.Died.</summary>
        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        /// <summary>Se desuscribe de Health.Died.</summary>
        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        private void OnDied()
        {
            GameFlowManager.Instance.NotifyPlayerDied();
        }
    }
}
