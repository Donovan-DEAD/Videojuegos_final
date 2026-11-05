using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Panel de historia entre mundos. Puede mostrar la barra como tiempo de página (modo por
    /// defecto) o como progreso externo de carga si se le pasa un proveedor.
    /// </summary>
    public class StoryInterludeController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image image;
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Text hint;
        [SerializeField] private Text pageLabel;
        [SerializeField] private AudioSource audioSource;

        [Header("Progress Bar")]
        [SerializeField] private Image progressFill;
        [SerializeField] private bool autoCreateProgressBar = true;
        [SerializeField] private Color progressColor = new Color(1f, 0.82f, 0.28f, 1f);
        [SerializeField] private Color progressBackgroundColor = new Color(0f, 0f, 0f, 0.65f);
        [SerializeField] private Vector2 progressSize = new Vector2(520f, 10f);
        [SerializeField] private float progressBottomOffset = 60f;

        private RectTransform progressRoot;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.sortingOrder = 10001;

            if (panel != null) panel.SetActive(false);
            if (group != null) group.alpha = 0f;
            EnsureProgressBar();
        }

        public void Play(StoryInterludeSO data, Action onComplete)
        {
            Play(data, onComplete, null);
        }

        /// <summary>
        /// Reproduce el interludio. Si <paramref name="progressProvider"/> no es null, la barra muestra
        /// ese progreso (0..1) en vez del tiempo de página; las páginas siguen avanzando por tiempo/ENTER.
        /// </summary>
        public void Play(StoryInterludeSO data, Action onComplete, Func<float> progressProvider)
        {
            if (data == null || data.Pages == null || data.Pages.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(Run(data, onComplete, progressProvider));
        }

        private IEnumerator Run(StoryInterludeSO data, Action onComplete, Func<float> progressProvider)
        {
            bool external = progressProvider != null;
            float displayed = 0f;

            IsPlaying = true;
            if (audioSource != null && data.Music != null) audioSource.PlayOneShot(data.Music);
            if (panel != null) panel.SetActive(true);
            EnsureProgressBar();
            if (group != null) { group.alpha = 0f; Tween.Alpha(group, 1f, 0.3f, useUnscaledTime: true); }
            if (title != null) title.text = data.Title;

            for (int i = 0; i < data.Pages.Length; i++)
            {
                var page = data.Pages[i];
                if (image != null) { image.sprite = page.image; image.enabled = page.image != null; }
                if (body != null) body.text = page.text;
                if (pageLabel != null) pageLabel.text = (i + 1) + " / " + data.Pages.Length;
                if (hint != null) hint.text = data.Skippable ? "ENTER para continuar" : "";

                float total = Mathf.Max(0.01f, page.holdSeconds);
                float t = 0f;
                if (!external) SetProgress(0f);
                while (t < total && !(data.Skippable && EnterPressed()))
                {
                    t += Time.unscaledDeltaTime;
                    if (external)
                    {
                        displayed = Mathf.MoveTowards(displayed, Mathf.Clamp01(progressProvider()), 1.25f * Time.unscaledDeltaTime);
                        SetProgress(displayed);
                    }
                    else
                    {
                        SetProgress(t / total);
                    }
                    yield return null;
                }
                if (!external) SetProgress(1f);
                yield return null;
            }

            if (group != null) Tween.Alpha(group, 0f, 0.25f, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(0.3f);
            if (panel != null) panel.SetActive(false);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        private void EnsureProgressBar()
        {
            if (progressFill != null)
            {
                ConfigureFill(progressFill);
                return;
            }
            if (!autoCreateProgressBar || panel == null) return;

            GameObject rootGo = new GameObject("ScreenProgressBar", typeof(RectTransform), typeof(CanvasRenderer));
            progressRoot = rootGo.GetComponent<RectTransform>();
            progressRoot.SetParent(panel.transform, false);
            progressRoot.anchorMin = new Vector2(0.5f, 0f);
            progressRoot.anchorMax = new Vector2(0.5f, 0f);
            progressRoot.pivot = new Vector2(0.5f, 0f);
            progressRoot.anchoredPosition = new Vector2(0f, progressBottomOffset);
            progressRoot.sizeDelta = progressSize;

            Image bg = rootGo.AddComponent<Image>();
            bg.color = progressBackgroundColor;
            bg.raycastTarget = false;

            GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.SetParent(progressRoot, false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);

            Image fill = fillGo.AddComponent<Image>();
            fill.color = progressColor;
            fill.raycastTarget = false;
            ConfigureFill(fill);
            progressFill = fill;
        }

        private static void ConfigureFill(Image fill)
        {
            if (fill == null) return;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
        }

        private void SetProgress(float value)
        {
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(value);
        }

        private static bool EnterPressed()
        {
            Keyboard k = Keyboard.current;
            if (k != null) return k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame;
            try { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter); } catch { return false; }
        }
    }
}
