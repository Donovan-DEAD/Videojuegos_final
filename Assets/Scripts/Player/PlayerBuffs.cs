using System;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Tipos de mejora temporal que puede vender el comerciante.</summary>
    public enum BuffKind
    {
        Damage = 0,
        Speed = 1,
        MaxHealth = 2
    }

    /// <summary>Gestiona mejoras temporales del jugador (stacks + duración).</summary>
    public class PlayerBuffs : MonoBehaviour
    {
        [SerializeField] private float damageDuration = 20f;
        [SerializeField] private float speedDuration = 20f;
        [SerializeField] private float healthDuration = 20f;
        [SerializeField] private float damagePerStack = 0.25f;
        [SerializeField] private float speedPerStack = 0.20f;
        [SerializeField] private float healthPerStack = 20f;

        private int[] stacks = new int[3];
        private float[] timers = new float[3];

        private PlayerStats stats;
        private PlayerController controller;
        private PlayerCombat combat;
        private Health health;

        /// <summary>Se dispara al cambiar stacks o temporizadores.</summary>
        public event Action BuffsChanged;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
        }

        public int GetStacks(BuffKind kind) => stacks[(int)kind];

        public float GetTimerNormalized(BuffKind kind)
        {
            float total = DurationOf(kind);
            return total > 0f ? Mathf.Clamp01(timers[(int)kind] / total) : 0f;
        }

        public void ApplyBuff(BuffKind kind)
        {
            stacks[(int)kind]++;
            timers[(int)kind] = DurationOf(kind);
            Reapply();
            BuffsChanged?.Invoke();
        }

        public void Reapply()
        {
            if (stats == null) return;

            float damageMult = 1f + stacks[(int)BuffKind.Damage] * damagePerStack;
            float speedMult = 1f + stacks[(int)BuffKind.Speed] * speedPerStack;
            float healthBonus = stacks[(int)BuffKind.MaxHealth] * healthPerStack;

            if (combat != null) combat.SetDamageMultiplier(stats.DamageMultiplier * damageMult);
            if (controller != null) controller.SetMoveSpeed(stats.MoveSpeed * speedMult);

            if (health != null)
            {
                int baseMax = Mathf.RoundToInt(stats.Health);
                int boosted = Mathf.RoundToInt(baseMax + healthBonus);
                health.SetMaxHealthPreserveCurrent(boosted);
            }
        }

        private float DurationOf(BuffKind kind)
        {
            switch (kind)
            {
                case BuffKind.Damage: return damageDuration;
                case BuffKind.Speed: return speedDuration;
                case BuffKind.MaxHealth: return healthDuration;
            }
            return 0f;
        }

        private void Update()
        {
            bool changed = false;
            for (int i = 0; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0) continue;

                timers[i] -= Time.deltaTime;
                if (timers[i] <= 0f)
                {
                    stacks[i]--;
                    timers[i] = stacks[i] > 0 ? DurationOf((BuffKind)i) : 0f;
                    changed = true;
                }
            }

            if (changed)
            {
                Reapply();
                BuffsChanged?.Invoke();
            }
        }
    }
}
