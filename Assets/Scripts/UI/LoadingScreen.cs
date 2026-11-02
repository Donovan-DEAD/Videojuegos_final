using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Pantalla de carga: carga de forma asíncrona la escena indicada por GameFlowManager.NextScene mostrando el progreso.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressLabel;
        [SerializeField] private float minimumTime = 0.75f;
        [SerializeField] private StoryInterludeSO debugStory;
        [SerializeField] private bool showDebugStory = false;

        /// <summary>
        /// Lee la escena destino desde GameFlowManager y arranca la carga asíncrona mostrando el progreso.
        /// </summary>
        private void Start()
        {
            string target = GameFlowManager.Instance != null ? GameFlowManager.Instance.NextScene : GameFlowManager.GameplayScene;
            Debug.Log($"[LoadingScreen] Start -> target '{target}'.");
            StartCoroutine(BootRoutine(target));
        }

        private IEnumerator BootRoutine(string target)
        {
            StoryInterludeSO interlude = GameFlowManager.Instance != null ? GameFlowManager.Instance.PendingStoryInterlude : null;
            if (interlude == null && showDebugStory) interlude = debugStory;
            if (interlude != null)
            {
                StoryInterludeController controller = FindAnyObjectByType<StoryInterludeController>();
                if (controller != null)
                {
                    bool done = false;
                    controller.Play(interlude, () => done = true);
                    while (!done) yield return null;
                    if (GameFlowManager.Instance != null) GameFlowManager.Instance.ClearPendingStoryInterlude();
                }
            }

            yield return LoadRoutine(target);
        }

        private IEnumerator LoadRoutine(string target)
        {
            if (minimumTime > 0f)
            {
                yield return new WaitForSecondsRealtime(minimumTime);
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(target, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"[LoadingScreen] No se pudo cargar '{target}'.");
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            SetProgress(1f);
            operation.allowSceneActivation = true;
        }

        private void SetProgress(float value)
        {
            if (progressFill != null)
            {
                progressFill.fillAmount = value;
            }

            if (progressLabel != null)
            {
                progressLabel.text = Mathf.RoundToInt(value * 100f) + "%";
            }
        }
    }
}
