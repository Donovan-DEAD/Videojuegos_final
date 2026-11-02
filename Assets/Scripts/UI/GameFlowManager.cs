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
        public const string CompletionScene = "06_Completion";
        public const string CreditsScene = "07_Credits";

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
        /// <summary>Historia a mostrar en la pantalla entre mundos.</summary>
        public StoryInterludeSO PendingStoryInterlude { get; private set; }
        /// <summary>
        /// Escena a la que volver al cerrar la pantalla de Ajustes.
        /// </summary>
        public string SettingsReturnScene { get; private set; } = MainMenuScene;
        /// <summary>
        /// Ajustes del usuario cargados desde preferencias.
        /// </summary>
        public SettingsData Settings { get; private set; }
        /// <summary>
        /// Nivel persistente del jugador durante la partida (sube al pasar de mundo).
        /// </summary>
        public int PlayerLevel { get; private set; } = 1;
        /// <summary>Vida actual del jugador para conservarla entre mundos.</summary>
        public int SavedPlayerHealth { get; private set; } = 0;
        public void SetSavedPlayerHealth(int value) { SavedPlayerHealth = Mathf.Max(0, value); }

        private readonly System.Collections.Generic.List<BossAbilitySO> savedAbilities = new System.Collections.Generic.List<BossAbilitySO>();
        public System.Collections.Generic.IReadOnlyList<BossAbilitySO> GetSavedAbilities() => savedAbilities;
        public void SetSavedAbilities(System.Collections.Generic.List<BossAbilitySO> abilities) { savedAbilities.Clear(); if (abilities != null) savedAbilities.AddRange(abilities); }
        public void ClearSavedAbilities() { savedAbilities.Clear(); }

        private readonly int[] savedPotions = new int[4];
        public int GetSavedPotion(int kind) => (kind >= 0 && kind < savedPotions.Length) ? savedPotions[kind] : 0;
        public void SetSavedPotion(int kind, int count) { if (kind >= 0 && kind < savedPotions.Length) savedPotions[kind] = Mathf.Max(0, count); }
        public void ClearSavedPotions() { for (int i = 0; i < savedPotions.Length; i++) savedPotions[i] = 0; }

        public bool HasSavedStats { get; private set; }
        public float SavedBaseMoveSpeed { get; private set; }
        public float SavedBaseDamageMultiplier { get; private set; }
        public float SavedBaseHealth { get; private set; }
        public int SavedLevel { get; private set; } = 1;

        public void SetSavedStats(float moveSpeed, float damage, float health, int level)
        {
            HasSavedStats = true;
            SavedBaseMoveSpeed = moveSpeed;
            SavedBaseDamageMultiplier = damage;
            SavedBaseHealth = health;
            SavedLevel = Mathf.Max(1, level);
            PlayerLevel = Mathf.Max(1, level);
        }
        private System.Collections.Generic.List<int> worldOrder = new System.Collections.Generic.List<int>();
        private int worldOrderIndex = 0;
        
        /// <summary>Mundos aún por visitar en la run.</summary>
        public int RemainingWorldCount => Mathf.Max(0, worldOrder.Count - worldOrderIndex);
        public bool HasWorldOrder => worldOrder != null && worldOrder.Count > 0;
        /// <summary>Fragmentos (moneda) acumulados en la run.</summary>
        public int Fragments { get; private set; }
        
        /// <summary>Se dispara al cambiar la cantidad de fragmentos.</summary>
        public event System.Action<int> FragmentsChanged;
        public event System.Action<int> FragmentsGained;
        
        /// <summary>Suma fragmentos y notifica el cambio.</summary>
        public void AddFragments(int amount)
        {
            int gained = Mathf.Max(0, amount);
            Fragments += gained;
            FragmentsChanged?.Invoke(Fragments);
            if (gained > 0) FragmentsGained?.Invoke(gained);
        }
        
        /// <summary>Gasta fragmentos si hay suficientes; false si no alcanza.</summary>
        public bool SpendFragments(int amount)
        {
            if (Fragments < amount)
            {
                return false;
            }
        
            Fragments -= amount;
            FragmentsChanged?.Invoke(Fragments);
            return true;
        }

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

        public void SetPendingStoryInterlude(StoryInterludeSO interlude)
        {
            PendingStoryInterlude = interlude;
        }

        public void ClearPendingStoryInterlude()
        {
            PendingStoryInterlude = null;
        }

        /// <summary>
        /// Navega a la escena del menú principal con transición.
        /// </summary>
        /// <summary>Sube un nivel al jugador (persistente entre mundos).</summary>
        public void LevelUp()
        {
            PlayerLevel++;
        }
        /// <summary>Baraja el orden de los mundos para una nueva run (sin repetir).</summary>
        public void BeginRun(int worldCount)
        {
            worldOrder = new System.Collections.Generic.List<int>(worldCount);
            for (int i = 0; i < worldCount; i++)
            {
                worldOrder.Add(i);
            }
        
            for (int i = worldOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = worldOrder[i];
                worldOrder[i] = worldOrder[j];
                worldOrder[j] = tmp;
            }
        
            worldOrderIndex = 0;
        }
        
        /// <summary>Devuelve y consume el siguiente índice de mundo sin repetir, o -1 si no quedan.</summary>
        public int ClaimNextWorld()
        {
            if (worldOrderIndex < worldOrder.Count)
            {
                return worldOrder[worldOrderIndex++];
            }
        
            return -1;
        }
        
        /// <summary>Reinicia la selección de mundos (al iniciar o reintentar una run).</summary>
        public void ResetWorldOrder()
        {
            worldOrder.Clear();
            worldOrderIndex = 0;
        }
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
            PlayerLevel = 1;
            SavedPlayerHealth = 0;
            ClearSavedAbilities();
            ClearSavedPotions();
            PlayerAbilities.ClearPersistent();
            ResetWorldOrder();
            Fragments = 0;
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
            PlayerLevel = 1;
            SavedPlayerHealth = 0;
            ClearSavedAbilities();
            ClearSavedPotions();
            PlayerAbilities.ClearPersistent();
            ResetWorldOrder();
            Fragments = 0;
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
        /// <summary>Notifica el fin de la run y va a la pantalla de finalización.</summary>
        public void NotifyRunCompleted()
        {
            RunCompleted?.Invoke();
            TransitionTo(CompletionScene);
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
