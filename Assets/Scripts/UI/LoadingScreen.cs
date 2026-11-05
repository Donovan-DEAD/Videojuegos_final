using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Pantalla de carga: monta la escena destino de forma aditiva mientras el interludio de historia
    /// sigue visible, muestra el progreso real (carga + generación del mundo) y desmonta la escena de
    /// carga cuando el mundo ya es jugable.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressLabel;
        [SerializeField] private float minimumTime = 1.75f;
        [SerializeField] private StoryInterludeSO debugStory;
        [SerializeField] private bool showDebugStory = false;

        /// <summary>True mientras el overlay de carga está visible (bloquea pausa/input).</summary>
        public static bool IsShowing { get; private set; }

        private Canvas loadingCanvas;
        private CanvasGroup canvasGroup;
        private AsyncOperation operation;
        private RunManager currentRun;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            ConfigureFill();
            EnsureOverlay();
        }

        private void ConfigureFill()
        {
            if (progressFill == null) return;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;

            if (progressFill.sprite == null)
            {
                Debug.LogWarning("[LoadingScreen] progressFill no tiene sprite: la barra no se animará.", this);
            }
        }

        private void EnsureOverlay()
        {
            loadingCanvas = progressFill != null ? progressFill.canvas : null;
            if (loadingCanvas == null) return;

            loadingCanvas.sortingOrder = 10000;

            canvasGroup = loadingCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = loadingCanvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;

            Transform canvasTransform = loadingCanvas.transform;
            if (canvasTransform.Find("InputBlocker") == null)
            {
                GameObject blocker = new GameObject("InputBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                blocker.transform.SetParent(canvasTransform, false);
                blocker.transform.SetAsFirstSibling();

                RectTransform rect = blocker.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                Image image = blocker.GetComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;
            }
        }

        private void Start()
        {
            string target = GameFlowManager.Instance != null ? GameFlowManager.Instance.NextScene : GameFlowManager.GameplayScene;
            Debug.Log($"[LoadingScreen] Start -> target '{target}'.");
            StartCoroutine(BootRoutine(target));
        }

        private IEnumerator BootRoutine(string target)
        {
            IsShowing = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            DisableLoadingSceneConflicts();

            operation = SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);
            if (operation == null)
            {
                Debug.LogError($"[LoadingScreen] No se pudo cargar '{target}'.");
                Finish(target);
                yield break;
            }

            operation.allowSceneActivation = false;
            currentRun = null;

            StoryInterludeSO interlude = GameFlowManager.Instance != null ? GameFlowManager.Instance.PendingStoryInterlude : null;
            if (interlude == null && showDebugStory) interlude = debugStory;

            bool interludeDone = interlude == null;
            StoryInterludeController controller = FindAnyObjectByType<StoryInterludeController>();
            if (interlude != null && controller != null)
            {
                controller.Play(interlude, () => interludeDone = true, ComputeProgress);
            }
            else
            {
                interludeDone = true;
            }

            float elapsed = 0f;

            while (operation.progress < 0.9f)
            {
                elapsed += Time.unscaledDeltaTime;
                SetProgress(ScenePhase(elapsed));
                yield return null;
            }

            while (elapsed < minimumTime)
            {
                elapsed += Time.unscaledDeltaTime;
                SetProgress(ScenePhase(elapsed));
                yield return null;
            }

            SetProgress(0.35f);
            operation.allowSceneActivation = true;

            int frames = 0;
            while (currentRun == null)
            {
                Scene targetScene = SceneManager.GetSceneByName(target);
                if (targetScene.IsValid() && targetScene.isLoaded)
                {
                    if (SceneManager.GetActiveScene() != targetScene)
                    {
                        SceneManager.SetActiveScene(targetScene);
                    }

                    currentRun = FindRunManager(targetScene);
                }

                if (currentRun == null)
                {
                    frames++;
                    if (frames > 180) break;
                }

                SetProgress(0.35f);
                yield return null;
            }

            if (currentRun != null)
            {
                while (!currentRun.IsWorldReady)
                {
                    SetProgress(0.35f + 0.65f * Mathf.Clamp01(currentRun.GenerationProgress));
                    yield return null;
                }
            }

            while (!interludeDone)
            {
                float ready = currentRun != null ? 0.35f + 0.65f * Mathf.Clamp01(currentRun.GenerationProgress) : 1f;
                SetProgress(ready);
                yield return null;
            }

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.ClearPendingStoryInterlude();
            }

            SetProgress(1f);
            yield return new WaitForSecondsRealtime(0.1f);
            yield return FadeOutRoutine(0.2f);

            Finish(target);
        }

        private RunManager FindRunManager(Scene scene)
        {
            RunManager[] managers = FindObjectsByType<RunManager>(FindObjectsSortMode.None);
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] != null && managers[i].gameObject.scene == scene)
                {
                    return managers[i];
                }
            }

            return null;
        }

        private void DisableLoadingSceneConflicts()
        {
            Scene loading = gameObject.scene;

            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] != null && listeners[i].gameObject.scene == loading)
                {
                    listeners[i].enabled = false;
                }
            }

            EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] != null && systems[i].gameObject.scene == loading)
                {
                    systems[i].enabled = false;
                }
            }
        }

        private float ComputeProgress()
        {
            if (operation == null)
            {
                return 0f;
            }

            float scene = Mathf.Clamp01(operation.progress / 0.9f);
            if (currentRun == null)
            {
                return 0.35f * scene;
            }

            return 0.35f + 0.65f * Mathf.Clamp01(currentRun.GenerationProgress);
        }

        private float ScenePhase(float elapsed)
        {
            if (operation == null)
            {
                return 0f;
            }

            float scene = Mathf.Clamp01(operation.progress / 0.9f);
            float time = minimumTime > 0f ? Mathf.Clamp01(elapsed / minimumTime) : 1f;
            return 0.35f * Mathf.Min(scene, time);
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            if (canvasGroup == null || duration <= 0f)
            {
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - t / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        private void Finish(string target)
        {
            IsShowing = false;
            Time.timeScale = previousTimeScale;

            Scene targetScene = SceneManager.GetSceneByName(target);
            if (targetScene.IsValid() && targetScene.isLoaded)
            {
                SceneManager.SetActiveScene(targetScene);
            }

            Scene loading = SceneManager.GetSceneByName(GameFlowManager.LoadingScene);
            if (loading.IsValid() && loading.isLoaded)
            {
                SceneManager.UnloadSceneAsync(loading);
            }
        }

        private void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            if (progressFill != null) progressFill.fillAmount = value;
            if (progressLabel != null) progressLabel.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        private void OnDestroy()
        {
            if (IsShowing)
            {
                IsShowing = false;
                Time.timeScale = previousTimeScale;
            }
        }
    }
}
