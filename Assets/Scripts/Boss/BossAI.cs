using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Jefe genérico derivado de <see cref="EnemyAI"/>; calcula fases según la salud,
    /// invoca esbirros al cambiar de fase y expone el contrato <see cref="IBossController"/>.
    /// </summary>
    public class BossAI : EnemyAI, IBossController
    {
        [SerializeField] private Spawner minionSpawner;
        [SerializeField] private GameObject[] minionPrefabs;
        [SerializeField] private int minionsPerSummon = 2;

        private BossStatsSO bossStats;
        private int currentPhase;
        private System.Random rng;

        public int CurrentPhase => currentPhase;

        /// <summary>
        /// Configura el jefe con sus estadísticas, objetivo y generador aleatorio, y aplica
        /// la configuración base de <see cref="EnemyAI"/>.
        /// </summary>
        public void ConfigureBoss(BossStatsSO bossStats, Transform target, System.Random random)
        {
            this.bossStats = bossStats;
            rng = random;
            Configure(bossStats, target);
        }

        /// <summary>
        /// Sobrecarga que configura el jefe sin un generador aleatorio explícito.
        /// </summary>
        public void ConfigureBoss(BossStatsSO bossStats, Transform target)
        {
            ConfigureBoss(bossStats, target, null);
        }

        /// <summary>
        /// Recupera las estadísticas de jefe si faltan, reinicia la fase y suscribe
        /// <c>OnHealthChanged</c> al evento <c>HealthChanged</c> de la salud.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (bossStats == null)
            {
                bossStats = stats as BossStatsSO;
            }

            currentPhase = 0;
            health.HealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            int newPhase = CalculatePhase(current, max);
            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                OnPhaseChanged(currentPhase);
            }
        }

        private int CalculatePhase(int currentHealth, int maxHealth)
        {
            if (bossStats == null || bossStats.PhaseHealthThresholds == null)
            {
                return 0;
            }

            float ratio = (float)currentHealth / Mathf.Max(1, maxHealth);
            float[] thresholds = bossStats.PhaseHealthThresholds;

            int phase = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (ratio <= thresholds[i])
                {
                    phase = i + 1;
                }
            }

            return phase;
        }

        protected virtual void OnPhaseChanged(int phase)
        {
            Debug.Log($"{name} entered phase {phase + 1}.");

            if (phase >= 1)
            {
                SummonMinions();
            }
        }

        private void SummonMinions()
        {
            if (minionSpawner == null || minionPrefabs == null || minionPrefabs.Length == 0)
            {
                return;
            }

            System.Random random = rng != null ? rng : new System.Random();

            for (int i = 0; i < minionsPerSummon; i++)
            {
                GameObject prefab = minionPrefabs[random.Next(minionPrefabs.Length)];
                minionSpawner.TrySpawn(prefab, transform.position, random, out _);
            }
        }
    }
}
