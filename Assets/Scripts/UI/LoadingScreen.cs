using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressLabel;
        [SerializeField] private float minimumTime = 0.75f;

        private void Start()
        {
            string target = GameFlowManager.Instance != null ? GameFlowManager.Instance.NextScene : GameFlowManager.GameplayScene;
            Debug.Log($"[LoadingScreen] Start -> target '{target}'.");
            StartCoroutine(LoadRoutine(target));
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
