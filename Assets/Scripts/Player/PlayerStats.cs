using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Define las estadísticas del jugador (velocidad, daño y vida) con crecimiento por nivel
    /// y las aplica a PlayerController, PlayerCombat y Health.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base stats")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseDamageMultiplier = 1f;
        [SerializeField] private float baseHealth = 100f;

        [Header("Growth per level")]
        [SerializeField] private float speedPerLevel = 0.25f;
        [SerializeField] private float damagePerLevel = 0.10f;
        [SerializeField] private float healthPerLevel = 10f;

        [Header("Level")]
        [SerializeField] private int level = 1;
        [SerializeField] private int maxLevel = 20;

        private PlayerController controller;
        private PlayerCombat combat;
        private Health health;

        public int Level => level;
        public int MaxLevel => maxLevel;
        public float BaseMoveSpeed => baseMoveSpeed;
        public float BaseDamageMultiplier => baseDamageMultiplier;
        public float BaseHealth => baseHealth;

        /// <summary>Velocidad de movimiento resultante para el nivel actual.</summary>
        public float MoveSpeed => baseMoveSpeed + speedPerLevel * (level - 1);
        /// <summary>Multiplicador de daño resultante para el nivel actual.</summary>
        public float DamageMultiplier => baseDamageMultiplier + damagePerLevel * (level - 1);
        /// <summary>Vida máxima resultante para el nivel actual.</summary>
        public float Health => baseHealth + healthPerLevel * (level - 1);

        /// <summary>Cachea los componentes y aplica las estadísticas iniciales.</summary>
        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
            Apply();
        }

        /// <summary>Fija el nivel (limitado a [1, MaxLevel]) y reaplica las estadísticas.</summary>
        public void SetLevel(int value)
        {
            level = Mathf.Clamp(value, 1, maxLevel);
            Apply();
        }

        /// <summary>Fija el nivel máximo.</summary>
        public void SetMaxLevel(int value)
        {
            maxLevel = Mathf.Max(1, value);
        }

        /// <summary>Fija la velocidad base (0.5-30) y reaplica las estadísticas.</summary>
        public void SetBaseMoveSpeed(float value)
        {
            baseMoveSpeed = Mathf.Clamp(value, 0.5f, 30f);
            Apply();
        }

        /// <summary>Fija el multiplicador de daño base (0.1-10) y reaplica las estadísticas.</summary>
        public void SetBaseDamageMultiplier(float value)
        {
            baseDamageMultiplier = Mathf.Clamp(value, 0.1f, 10f);
            Apply();
        }

        /// <summary>Fija la vida base (1-1000) y reaplica las estadísticas.</summary>
        public void SetBaseHealth(float value)
        {
            baseHealth = Mathf.Clamp(value, 1f, 1000f);
            Apply();
        }

        /// <summary>Propaga las estadísticas resultantes a PlayerController, PlayerCombat y Health.</summary>
        public void Apply()
        {
            if (controller != null)
            {
                controller.SetMoveSpeed(MoveSpeed);
            }

            if (combat != null)
            {
                combat.SetDamageMultiplier(DamageMultiplier);
            }

            if (health != null)
            {
                health.SetMaxHealth(Mathf.RoundToInt(Health));
            }
        }
    }
}
