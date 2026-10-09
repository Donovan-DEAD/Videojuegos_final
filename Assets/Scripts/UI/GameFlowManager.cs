using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadnessDreams
{
    public enum CharacterId
    {
        Hansel = 0,
        Gretel = 1
    }

    public class GameFlowManager : MonoBehaviour
    {
        public const string MainMenuScene = "01_MainMenu";
        public const string LoadingScene = "02_Loading";
        public const string GameplayScene = "03_Gameplay";
        public const string DefeatScene = "04_Defeat";
        public const string SettingsScene = "05_Settings";

        private static GameFlowManager instance;

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
        public int RunSeed { get; private set; }
        public int CurrentWorldIndex { get; private set; }
        public int PendingWorldIndex { get; private set; }
        public string NextScene { get; private set; } = GameplayScene;
        public string SettingsReturnScene { get; private set; } = MainMenuScene;
        public SettingsData Settings { get; private set; }

        public event System.Action PlayerDied;
        public event System.Action RunCompleted;

        [SerializeField] private float fadeOutDuration = 0.25f;

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

        public void SetPendingWorldIndex(int index)
        {
            PendingWorldIndex = Mathf.Max(0, index);
        }

        public void GoToMainMenu()
        {
            TransitionTo(MainMenuScene);
        }

        public void StartRun(CharacterId character)
        {
            SelectedCharacter = character;
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
            CurrentWorldIndex = 0;
            PendingWorldIndex = 0;
            LoadSceneWithLoading(GameplayScene);
        }

        public void Retry()
        {
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
            CurrentWorldIndex = 0;
            PendingWorldIndex = 0;
            LoadSceneWithLoading(GameplayScene);
        }

        public void GoToDefeat()
        {
            TransitionTo(DefeatScene);
        }

        public void OpenSettings(string fromScene)
        {
            SettingsReturnScene = string.IsNullOrEmpty(fromScene) ? MainMenuScene : fromScene;
            TransitionTo(SettingsScene);
        }

        public void CloseSettings()
        {
            TransitionTo(string.IsNullOrEmpty(SettingsReturnScene) ? MainMenuScene : SettingsReturnScene);
        }

        public void LoadSceneWithLoading(string targetScene)
        {
            NextScene = string.IsNullOrEmpty(targetScene) ? GameplayScene : targetScene;
            TransitionTo(LoadingScene);
        }

        public void NotifyPlayerDied()
        {
            PlayerDied?.Invoke();
            GoToDefeat();
        }

        public void NotifyRunCompleted()
        {
            RunCompleted?.Invoke();
            GoToMainMenu();
        }

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
