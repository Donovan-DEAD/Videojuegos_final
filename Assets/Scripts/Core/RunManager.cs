using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Orquesta una partida: genera cada mundo con el <c>MapGenerator</c> y el <see cref="BossManager"/>,
    /// y avanza al siguiente mundo o cierra la run al derrotar al jefe.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private List<WorldConfigSO> worlds = new List<WorldConfigSO>();
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private BossManager bossManager;

        public WorldConfigSO CurrentWorld { get; private set; }
        public int CurrentWorldIndex { get; private set; }
        public int WorldCount => worlds != null ? worlds.Count : 0;

        /// <summary>
        /// Se suscribe a la derrota del jefe y genera el mundo inicial indicado por <c>GameFlowManager</c>.
        /// </summary>
        private void Awake()
        {
            if (bossManager != null)
            {
                bossManager.BossDefeated += OnBossDefeated;
            }

            int startIndex = GameFlowManager.Instance != null ? GameFlowManager.Instance.PendingWorldIndex : 0;
            GenerateWorld(startIndex);
        }

        /// <summary>
        /// Se desuscribe del evento de jefe derrotado al destruirse.
        /// </summary>
        private void OnDestroy()
        {
            if (bossManager != null)
            {
                bossManager.BossDefeated -= OnBossDefeated;
            }
        }

        /// <summary>
        /// Fija y genera el mundo indicado (índice limitado al rango válido), reconfigurando
        /// el generador de mapa y el jefe.
        /// </summary>
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

                // Semilla por run: la misma config produce un mapa distinto en cada partida.
                int runSeed = GameFlowManager.Instance != null ? GameFlowManager.Instance.RunSeed : CurrentWorld.Seed;
                mapGenerator.SetSeed(runSeed + CurrentWorldIndex * 7919);
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

        /// <summary>
        /// Avanza al siguiente mundo si existe o notifica que la run se completó, cargando la
        /// escena de juego mediante <c>GameFlowManager</c>.
        /// </summary>
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

                return;
            }

            // Un solo mundo de ejemplo: reiniciar la run con una semilla nueva mantiene el portal con sentido.
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.Retry();
            }
        }
    }
}
