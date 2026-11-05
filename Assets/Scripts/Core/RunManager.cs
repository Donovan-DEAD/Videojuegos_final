using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadnessDreams
{
    /// <summary>
    /// Orquesta una partida: prepara cada mundo, genera su mapa de forma incremental con el
    /// <c>MapGenerator</c> y el <see cref="BossManager"/>, y avanza al siguiente mundo o cierra la run
    /// al derrotar al jefe. Expone el progreso de generación para la pantalla de carga.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private List<WorldConfigSO> worlds = new List<WorldConfigSO>();
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private BossManager bossManager;

        public WorldConfigSO CurrentWorld { get; private set; }
        public int CurrentWorldIndex { get; private set; }
        public int WorldCount => worlds != null ? worlds.Count : 0;

        /// <summary>Progreso de generación del mundo actual (0..1).</summary>
        public float GenerationProgress { get; private set; }
        /// <summary>True cuando el mundo terminó de generarse y ya es jugable.</summary>
        public bool IsWorldReady { get; private set; }
        /// <summary>Se dispara cuando el mundo terminó de generarse.</summary>
        public event System.Action WorldReady;

        /// <summary>
        /// Se suscribe a la derrota del jefe, elige el mundo inicial y lo prepara (sin generar).
        /// La generación corre en <see cref="Start"/> para poder reportar progreso.
        /// </summary>
        private void Awake()
        {
            // La escena de gameplay se carga de forma aditiva desde 02_Loading; activarla antes de
            // generar garantiza que props/enemigos instanciados pertenezcan a esta escena y no a la
            // pantalla de carga (que se descarga al terminar).
            if (gameObject.scene.IsValid() && gameObject.scene.isLoaded)
            {
                SceneManager.SetActiveScene(gameObject.scene);
            }

            if (bossManager != null)
            {
                bossManager.BossDefeated += OnBossDefeated;
            }

            int startIndex = 0;
            if (GameFlowManager.Instance != null)
            {
                if (!GameFlowManager.Instance.HasWorldOrder)
                {
                    GameFlowManager.Instance.BeginRun(worlds.Count);
                }

                int next = GameFlowManager.Instance.ClaimNextWorld();
                if (next >= 0)
                {
                    startIndex = next;
                }
            }

            PrepareWorld(startIndex);
        }

        /// <summary>Arranca la generación incremental del mundo preparado.</summary>
        private void Start()
        {
            StartCoroutine(GenerateWorldRoutine());
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
        /// Fija el mundo indicado y configura el generador y el jefe, sin generar todavía.
        /// </summary>
        private void PrepareWorld(int index)
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
            }

            if (bossManager != null)
            {
                bossManager.Configure(CurrentWorld);
            }
        }

        /// <summary>
        /// Genera el mundo repartiendo el trabajo en frames, configura el jefe y marca el mundo como listo.
        /// </summary>
        private IEnumerator GenerateWorldRoutine()
        {
            IsWorldReady = false;
            GenerationProgress = 0f;

            if (mapGenerator != null)
            {
                yield return mapGenerator.GenerateRoutine(progress => GenerationProgress = progress);
            }

            if (bossManager != null)
            {
                bossManager.SetupBossFight();
            }

            GenerationProgress = 1f;
            IsWorldReady = true;
            WorldReady?.Invoke();
        }

        /// <summary>
        /// Genera el mundo indicado de forma sincrónica (editor/pruebas) y marca el mundo como listo.
        /// </summary>
        public void GenerateWorld(int index)
        {
            PrepareWorld(index);

            if (mapGenerator != null)
            {
                mapGenerator.Generate();
            }

            if (bossManager != null)
            {
                bossManager.SetupBossFight();
            }

            GenerationProgress = 1f;
            IsWorldReady = true;
            WorldReady?.Invoke();
        }

        private void OnBossDefeated()
        {
            Debug.Log($"World {CurrentWorldIndex + 1} cleared. Busca el portal.");
        }

        /// <summary>
        /// Avanza al siguiente mundo si existe o notifica que la run se completó, cargando la
        /// escena de juego mediante <c>GameFlowManager</c>. Usa el interludio del mundo que se entra.
        /// </summary>
        public void RequestAdvance()
        {
            if (GameFlowManager.Instance == null)
            {
                return;
            }

            if (GameFlowManager.Instance.RemainingWorldCount <= 0)
            {
                GameFlowManager.Instance.NotifyRunCompleted();
                return;
            }

            int nextIndex = GameFlowManager.Instance.PeekNextWorld();
            StoryInterludeSO interlude = null;

            if (nextIndex >= 0 && worlds != null && nextIndex < worlds.Count && worlds[nextIndex] != null)
            {
                interlude = worlds[nextIndex].StoryInterlude;
            }

            if (interlude == null && CurrentWorld != null)
            {
                interlude = CurrentWorld.StoryInterlude;
            }

            GameFlowManager.Instance.LevelUp();
            GameFlowManager.Instance.SetPendingStoryInterlude(interlude);
            GameFlowManager.Instance.LoadSceneWithLoading(GameFlowManager.GameplayScene);
        }
    }
}
