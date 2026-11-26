using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadnessDreams
{
    /// <summary>
    /// Música de fondo persistente: una sola fuente con fundido entre pistas.
    /// Escucha el ciclo de vida existente (escenas, RunManager.WorldReady y
    /// BossArenaController) sin modificar lógica de juego ni crear escenas.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        public const string MainMenuCue = "MainMenu";
        public const string ForestWorldCue = "RedRidingHood_World";
        public const string ForestBossCue = "RedRidingHood_Boss";
        public const string SeaWorldCue = "PeterPan_World";
        public const string SeaBossCue = "PeterPan_Boss";

        [Header("Clips")]
        [SerializeField] private AudioClip mainMenu;
        [SerializeField] private AudioClip forestWorld;
        [SerializeField] private AudioClip forestBoss;
        [SerializeField] private AudioClip seaWorld;
        [SerializeField] private AudioClip seaBoss;

        [Header("Mix")]
        [SerializeField] private float musicVolume = 0.45f;
        [SerializeField] private float fadeDuration = 0.9f;

        private static MusicManager instance;

        private AudioSource source;
        private string currentCue;
        private string worldCue;
        private Coroutine fadeRoutine;
        private RunManager runManager;
        private BossArenaController arena;
        private bool subscribedArena;

        public static MusicManager Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindFirstObjectByType<MusicManager>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            EnsureSource();
        }

        /// <summary>
        /// La fuente puede perderse en una recarga de escena, así que se re-adquiere
        /// bajo demanda en lugar de depender de un único Awake.
        /// </summary>
        private AudioSource EnsureSource()
        {
            if (source != null) return source;

            AudioSource[] existing = GetComponents<AudioSource>();
            source = existing.Length > 0 ? existing[0] : gameObject.AddComponent<AudioSource>();

            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            if (!source.isPlaying) source.volume = 0f;
            return source;
        }

        private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
        private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            arena = null;
            subscribedArena = false;
            BindRunManager();

            switch (scene.name)
            {
                case GameFlowManager.MainMenuScene:
                    worldCue = null;
                    PlayCue(MainMenuCue);
                    break;

                case GameFlowManager.GameplayScene:
                    worldCue = ResolveWorldCue();
                    PlayCue(worldCue);
                    break;
            }
        }

        private void BindRunManager()
        {
            runManager = FindFirstObjectByType<RunManager>();
            if (runManager == null) return;
            runManager.WorldReady -= OnWorldReady;
            runManager.WorldReady += OnWorldReady;
        }

        private void OnWorldReady()
        {
            worldCue = ResolveWorldCue();
            if (SceneManager.GetActiveScene().name == GameFlowManager.GameplayScene)
            {
                PlayCue(worldCue);
            }
        }

        /// <summary>El mundo actual decide la pista: mar -> Peter Pan, resto -> Caperucita.</summary>
        private string ResolveWorldCue()
        {
            if (runManager == null) runManager = FindFirstObjectByType<RunManager>();
            if (runManager == null || runManager.CurrentWorld == null) return ForestWorldCue;
            return runManager.CurrentWorld.name.Contains("Sea") ? SeaWorldCue : ForestWorldCue;
        }

        /// <summary>
        /// Engancha el aria de jefe cuando aparece en escena: al spawnear cambia a la
        /// pista de jefe, al morir vuelve a la música de exploración del mundo.
        /// </summary>
        private void Update()
        {
            if (arena != null || subscribedArena) return;
            arena = FindFirstObjectByType<BossArenaController>();
            if (arena == null) return;

            arena.BossSpawned += OnBossSpawned;
            arena.BossDefeated += OnBossDefeated;
            subscribedArena = true;
        }

        private void OnBossSpawned(Health health, string bossName)
        {
            PlayCue(worldCue == SeaWorldCue ? SeaBossCue : ForestBossCue);
        }

        private void OnBossDefeated()
        {
            if (SceneManager.GetActiveScene().name == GameFlowManager.GameplayScene)
            {
                PlayCue(worldCue != null ? worldCue : ResolveWorldCue());
            }
        }

        /// <summary>Reproduce una pista con fundido cruzado; ignora la petición si ya suena.</summary>
        public void PlayCue(string cue)
        {
            AudioClip clip = ClipFor(cue);
            if (clip == null) return;

            AudioSource src = EnsureSource();
            if (currentCue == cue && src.isPlaying) return;

            currentCue = cue;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeTo(clip));
        }

        private IEnumerator FadeTo(AudioClip clip)
        {
            AudioSource src = EnsureSource();
            float start = src.volume;

            if (src.isPlaying && start > 0.001f)
            {
                for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / fadeDuration)
                {
                    src.volume = Mathf.Lerp(start, 0f, t);
                    yield return null;
                }
                src.volume = 0f;
            }

            src.Stop();
            src.clip = clip;
            src.volume = 0f;
            src.Play();

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / fadeDuration)
            {
                src.volume = Mathf.Lerp(0f, musicVolume, t);
                yield return null;
            }

            src.volume = musicVolume;
            fadeRoutine = null;
        }

        private AudioClip ClipFor(string cue)
        {
            switch (cue)
            {
                case MainMenuCue:    return mainMenu;
                case ForestWorldCue: return forestWorld;
                case ForestBossCue:  return forestBoss;
                case SeaWorldCue:    return seaWorld;
                case SeaBossCue:     return seaBoss;
                default:             return null;
            }
        }
    }
}
