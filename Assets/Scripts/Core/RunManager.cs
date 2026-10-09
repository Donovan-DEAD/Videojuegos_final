using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private List<WorldConfigSO> worlds = new List<WorldConfigSO>();
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private BossManager bossManager;

        public WorldConfigSO CurrentWorld { get; private set; }
        public int CurrentWorldIndex { get; private set; }
        public int WorldCount => worlds != null ? worlds.Count : 0;

        private void Start()
        {
            if (bossManager != null)
            {
                bossManager.BossDefeated += OnBossDefeated;
            }

            int startIndex = GameFlowManager.Instance != null ? GameFlowManager.Instance.PendingWorldIndex : 0;
            GenerateWorld(startIndex);
        }

        private void OnDestroy()
        {
            if (bossManager != null)
            {
                bossManager.BossDefeated -= OnBossDefeated;
            }
        }

        public void GenerateWorld(int index)
        {
            if (worlds == null || worlds.Count == 0)
            {
                Debug.LogError("RunManager: no worlds configured.", this);
                return;
            }

            CurrentWorldIndex = Mathf.Clamp(index, 0, worlds.Count - 1);
            CurrentWorld = worlds[CurrentWorldIndex];

            if (mapGenerator != null)
            {
                mapGenerator.Configure(CurrentWorld);
                mapGenerator.Generate();
            }

            if (bossManager != null)
            {
                bossManager.Configure(CurrentWorld);
                bossManager.SetupBossFight();
            }
        }

        private void OnBossDefeated()
        {
            Debug.Log($"World {CurrentWorldIndex + 1} cleared. Busca el portal.");
        }

        public void RequestAdvance()
        {
            if (worlds == null || worlds.Count == 0)
            {
                return;
            }

            int next = CurrentWorldIndex + 1;
            if (next < worlds.Count)
            {
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.SetPendingWorldIndex(next);
                    GameFlowManager.Instance.LoadSceneWithLoading(GameFlowManager.GameplayScene);
                }
            }
            else
            {
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.NotifyRunCompleted();
                }
            }
        }
    }
}
