using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadnessDreams
{
    /// <summary>
    /// Identifica al protagonista de la partida (Hansel o Gretel).
    /// </summary>
    public enum CharacterId
    {
        Hansel = 0,
        Gretel = 1
    }

    /// <summary>
    /// Singleton persistente (se autocrea bajo demanda) que mantiene el estado de la partida (personaje, semilla, índice de mundo)
    /// y navega entre escenas mediante TransitionTo con fundido de ScreenFader.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public const string MainMenuScene = "01_MainMenu";
        public const string LoadingScene = "02_Loading";
        public const string GameplayScene = "03_Gameplay";
        public const string DefeatScene = "04_Defeat";
        public const string SettingsScene = "05_Settings";

        private static GameFlowManager instance;

        /// <summary>
        /// Singleton de acceso global: si no existe lo busca o lo crea y lo marca como DontDestroyOnLoad.
        /// </summary>
        public static GameFlowManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<GameFlowManager>();

                    if (instance == null)
                    {
                        GameObject host = new GameObject("GameFlowManager");
                        instance = host.AddComponent<GameFlowManager>();
                    }

                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
            }
        }

        public CharacterId SelectedCharacter { get; private set; } = CharacterId.Hansel;
        /// <summary>
        /// Semilla aleatoria de la partida actual, usada para generar los mundos.
        /// </summary>
        public int RunSeed { get; private set; }
        /// <summary>
        /// Índice del mundo en curso dentro de la partida (run).
        /// </summary>
        public int CurrentWorldIndex { get; private set; }
        /// <summary>
        /// Índice del mundo a cargar en la siguiente transición.
        /// </summary>
        public int PendingWorldIndex { get; private set; }
        /// <summary>
        /// Escena destino que cargará la pantalla de carga.
        /// </summary>
        public string NextScene { get; private set; } = GameplayScene;
        /// <summary>
        /// Escena a la que volver al cerrar la pantalla de Ajustes.
        /// </summary>
        public string SettingsReturnScene { get; private set; } = MainMenuScene;
        /// <summary>
        /// Ajustes del usuario cargados desde preferencias.
        /// </summary>
        public SettingsData Settings { get; private set; }

        /// <summary>
        /// Se dispara cuando el jugador muere, antes de ir a la escena de derrota.
        /// </summary>
        public event System.Action PlayerDied;
        /// <summary>
        /// Se dispara al completar la partida (run), antes de volver al menú.
        /// </summary>
        public event System.Action RunCompleted;

        [SerializeField] private float fadeOutDuration = 0.25f;

        /// <summary>
        /// Aplica el patrón singleton persistente y carga y aplica los ajustes guardados.
        /// </summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Settings = SettingsData.Load();
            Settings.Apply();
        }

        /// <summary>
        /// Fija el índice del mundo pendiente, nunca negativo.
        /// </summary>
        public void SetPendingWorldIndex(int index)
        {
            PendingWorldIndex = Mathf.Max(0, index);
        }

        /// <summary>
        /// Navega a la escena del menú principal con transición.
        /// </summary>
        public void GoToMainMenu()
        {
            TransitionTo(MainMenuScene);
        }

        /// <summary>
        /// Inicia una nueva partida con el personaje indicado: genera semilla, reinicia los índices y carga Gameplay con pantalla de carga.
        /// </summary>
        public void StartRun(CharacterId character)
        {
            SelectedCharacter = character;
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
            CurrentWorldIndex = 0;
            PendingWorldIndex = 0;
            LoadSceneWithLoading(GameplayScene);
        }

        /// <summary>
        /// Reinicia la partida con una nueva semilla y vuelve a cargar Gameplay.
        /// </summary>
        public void Retry()
        {
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
            CurrentWorldIndex = 0;
            PendingWorldIndex = 0;
            LoadSceneWithLoading(GameplayScene);
        }

        /// <summary>
        /// Navega a la escena de derrota con transición.
        /// </summary>
        public void GoToDefeat()
        {
            TransitionTo(DefeatScene);
        }

        /// <summary>
        /// Abre Ajustes recordando la escena de origen para poder volver a ella.
        /// </summary>
        public void OpenSettings(string fromScene)
        {
            SettingsReturnScene = string.IsNullOrEmpty(fromScene) ? MainMenuScene : fromScene;
            TransitionTo(SettingsScene);
        }

        /// <summary>
        /// Vuelve a la escena de origen de Ajustes (o al menú si no hay).
        /// </summary>
        public void CloseSettings()
        {
            TransitionTo(string.IsNullOrEmpty(SettingsReturnScene) ? MainMenuScene : SettingsReturnScene);
        }

        /// <summary>
        /// Fija la escena destino y navega a la escena de carga (Loading).
        /// </summary>
        public void LoadSceneWithLoading(string targetScene)
        {
            NextScene = string.IsNullOrEmpty(targetScene) ? GameplayScene : targetScene;
            TransitionTo(LoadingScene);
        }

        /// <summary>
        /// Notifica la muerte del jugador (dispara PlayerDied) y va a la escena de derrota.
        /// </summary>
        public void NotifyPlayerDied()
        {
            PlayerDied?.Invoke();
            GoToDefeat();
        }

        /// <summary>
        /// Notifica el fin de la run (dispara RunCompleted) y vuelve al menú principal.
        /// </summary>
        public void NotifyRunCompleted()
        {
            RunCompleted?.Invoke();
            GoToMainMenu();
        }

        /// <summary>
        /// Cierra la aplicación (detiene el modo Play en el editor).
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void TransitionTo(string scene)
        {
            if (!isActiveAndEnabled)
            {
                SceneManager.LoadScene(scene);
                return;
            }

            StartCoroutine(TransitionRoutine(scene));
        }

        private IEnumerator TransitionRoutine(string scene)
        {
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeOut(fadeOutDuration);
            }

            SceneManager.LoadScene(scene);
        }
    }
}
