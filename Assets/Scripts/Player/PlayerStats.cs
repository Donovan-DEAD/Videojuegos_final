using UnityEngine;

namespace MadnessDreams
{
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

        public float MoveSpeed => baseMoveSpeed + speedPerLevel * (level - 1);
        public float DamageMultiplier => baseDamageMultiplier + damagePerLevel * (level - 1);
        public float Health => baseHealth + healthPerLevel * (level - 1);

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
            Apply();
        }

        public void SetLevel(int value)
        {
            level = Mathf.Clamp(value, 1, maxLevel);
            Apply();
        }

        public void SetMaxLevel(int value)
        {
            maxLevel = Mathf.Max(1, value);
        }

        public void SetBaseMoveSpeed(float value)
        {
            baseMoveSpeed = Mathf.Clamp(value, 0.5f, 30f);
            Apply();
        }

        public void SetBaseDamageMultiplier(float value)
        {
            baseDamageMultiplier = Mathf.Clamp(value, 0.1f, 10f);
            Apply();
        }

        public void SetBaseHealth(float value)
        {
            baseHealth = Mathf.Clamp(value, 1f, 1000f);
            Apply();
        }

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
