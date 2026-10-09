using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Cualquier objeto que pueda recibir daño.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }

    /// <summary>
    /// Gestiona los puntos de vida: aplica daño, limita los valores y emite los eventos de
    /// cambio de salud y de muerte.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        /// <summary>
        /// Se dispara al cambiar la salud, con (salud actual, salud máxima).
        /// </summary>
        public event System.Action<int, int> HealthChanged;
        /// <summary>
        /// Se dispara una sola vez cuando la salud llega a cero.
        /// </summary>
        public event System.Action Died;

        /// <summary>
        /// Inicializa la salud actual al valor máximo configurado.
        /// </summary>
        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        /// <summary>
        /// Fija la salud máxima (mínimo 1), restablece la salud actual y emite <see cref="HealthChanged"/>.
        /// </summary>
        public void SetMaxHealth(int value)
        {
            maxHealth = Mathf.Max(1, value);
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>
        /// Resta daño sin bajar de cero, emite <see cref="HealthChanged"/> y dispara <see cref="Died"/> al llegar a cero.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (IsDead)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
            {
                Died?.Invoke();
            }
        }
    }
}
