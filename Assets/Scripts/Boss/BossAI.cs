using UnityEngine;

namespace MadnessDreams
{
    public class BossAI : EnemyAI, IBossController
    {
        [SerializeField] private Spawner minionSpawner;
        [SerializeField] private GameObject[] minionPrefabs;
        [SerializeField] private int minionsPerSummon = 2;

        private BossStatsSO bossStats;
        private int currentPhase;
        private System.Random rng;

        public int CurrentPhase => currentPhase;

        public void ConfigureBoss(BossStatsSO bossStats, Transform target, System.Random random)
        {
            this.bossStats = bossStats;
            rng = random;
            Configure(bossStats, target);
        }

        public void ConfigureBoss(BossStatsSO bossStats, Transform target)
        {
            ConfigureBoss(bossStats, target, null);
        }

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
