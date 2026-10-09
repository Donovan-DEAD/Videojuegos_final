using UnityEngine;

namespace MadnessDreams
{
    public class BossManager : MonoBehaviour
    {
        [SerializeField] private WorldConfigSO config;
        [SerializeField] private BossArenaController arena;

        public event System.Action BossDefeated;

        public void Configure(WorldConfigSO worldConfig)
        {
            config = worldConfig;
        }

        public void SetupBossFight()
        {
            if (arena == null || config == null)
            {
                return;
            }

            arena.Setup(config.BossPrefab, config.Boss);
            arena.BossDefeated += OnBossDefeated;
        }

        private void OnBossDefeated()
        {
            Debug.Log("Boss defeated.");
            BossDefeated?.Invoke();
        }

        private void OnDestroy()
        {
            if (arena != null)
            {
                arena.BossDefeated -= OnBossDefeated;
            }
        }
    }
}
