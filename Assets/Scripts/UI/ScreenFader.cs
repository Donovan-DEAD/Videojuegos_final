using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Fundido a negro persistente (singleton creado bajo demanda): construye su propio canvas y hace FadeIn al cargar cada escena.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        private static ScreenFader instance;

        /// <summary>
        /// Singleton de acceso global: si no existe lo busca o lo crea y lo marca como DontDestroyOnLoad.
        /// </summary>
        public static ScreenFader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<ScreenFader>();

                    if (instance == null)
                    {
                        GameObject host = new GameObject("ScreenFader");
                        instance = host.AddComponent<ScreenFader>();
                    }

                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
            }
        }

        [SerializeField] private float defaultFadeInDuration = 0.35f;

        private Image fadeImage;

        /// <summary>
        /// Aplica el patrón singleton persistente, construye el canvas del fundido y se suscribe a la carga de escenas.
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
            BuildCanvas();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Deja de escuchar la carga de escenas al destruirse.
        /// </summary>
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var imgGo = new GameObject("Fade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imgGo.transform.SetParent(canvasGo.transform, false);
            var rt = imgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            fadeImage = imgGo.GetComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(FadeIn(defaultFadeInDuration));
        }

        /// <summary>
        /// Corrutina que funde la pantalla a negro durante <paramref name="duration"/>.
        /// </summary>
        public IEnumerator FadeOut(float duration)
        {
            yield return Fade(CurrentAlpha(), 1f, duration);
        }

        /// <summary>
        /// Corrutina que funde la pantalla de negro a transparente durante <paramref name="duration"/>.
        /// </summary>
        public IEnumerator FadeIn(float duration)
        {
            yield return Fade(CurrentAlpha(), 0f, duration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (fadeImage == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                SetAlpha(to);
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, t / duration));
                yield return null;
            }

            SetAlpha(to);
        }

        private float CurrentAlpha()
        {
            return fadeImage != null ? fadeImage.color.a : 0f;
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage == null)
            {
                return;
            }

            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            fadeImage.raycastTarget = alpha > 0.001f;
        }
    }
}
