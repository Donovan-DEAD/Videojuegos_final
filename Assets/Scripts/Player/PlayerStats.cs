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
        private PlayerBuffs buffs;

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
            buffs = GetComponent<PlayerBuffs>();
        
            bool restore = false;
            if (GameFlowManager.Instance != null)
            {
                level = Mathf.Clamp(GameFlowManager.Instance.PlayerLevel, 1, maxLevel);
                restore = GameFlowManager.Instance.SavedPlayerHealth > 0;
        
                if (GameFlowManager.Instance.HasSavedStats)
                {
                    baseMoveSpeed = GameFlowManager.Instance.SavedBaseMoveSpeed;
                    baseDamageMultiplier = GameFlowManager.Instance.SavedBaseDamageMultiplier;
                    baseHealth = GameFlowManager.Instance.SavedBaseHealth;
                    level = Mathf.Clamp(GameFlowManager.Instance.SavedLevel, 1, maxLevel);
                }
            }
        
            Apply(restore);
        }

        private void Start()
        {
            if (health != null && GameFlowManager.Instance != null && GameFlowManager.Instance.SavedPlayerHealth > 0)
            {
                health.SetCurrentHealth(GameFlowManager.Instance.SavedPlayerHealth);
            }
        }

        private void OnDestroy()
        {
            if (health != null && GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetSavedPlayerHealth(health.CurrentHealth);
            }
        }

        /// <summary>Fija el nivel (limitado a [1, MaxLevel]) y reaplica las estadísticas.</summary>
        public void SetLevel(int value)
        {
            level = Mathf.Clamp(value, 1, maxLevel);
            Apply();
            SaveStats();
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
            SaveStats();
        }

        /// <summary>Fija el multiplicador de daño base (0.1-10) y reaplica las estadísticas.</summary>
        public void SetBaseDamageMultiplier(float value)
        {
            baseDamageMultiplier = Mathf.Clamp(value, 0.1f, 10f);
            Apply();
            SaveStats();
        }

        /// <summary>Fija la vida base (1-1000) y reaplica las estadísticas.</summary>
        public void SetBaseHealth(float value)
        {
            baseHealth = Mathf.Clamp(value, 1f, 1000f);
            Apply();
            SaveStats();
        }

        /// <summary>Propaga las estadísticas resultantes a PlayerController, PlayerCombat y Health.</summary>
        private void SaveStats()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetSavedStats(baseMoveSpeed, baseDamageMultiplier, baseHealth, level);
            }
        }

        public void Apply(bool preserveHealth = false)
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
                if (preserveHealth) health.SetMaxHealthPreserveCurrent(Mathf.RoundToInt(Health));
                else health.SetMaxHealth(Mathf.RoundToInt(Health));
                if (buffs != null)
                {
                    buffs.Reapply();
                }
            }
        }
    }
}
