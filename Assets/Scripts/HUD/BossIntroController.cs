using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class BossIntroController : MonoBehaviour
    {
        public static BossIntroController Instance { get; private set; }
        public static bool IsShowing { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image background;
        [SerializeField] private Image portrait;
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private Text hint;
        [SerializeField] private AudioSource audioSource;

        [Header("Progress Bar")]
        [SerializeField] private Image progressFill;
        [SerializeField] private bool autoCreateProgressBar = true;
        [SerializeField] private Color progressColor = new Color(1f, 0.82f, 0.28f, 1f);
        [SerializeField] private Color progressBackgroundColor = new Color(0f, 0f, 0f, 0.65f);
        [SerializeField] private Vector2 progressSize = new Vector2(520f, 10f);
        [SerializeField] private float progressBottomOffset = 70f;

        private Coroutine routine;
        private float previousScale = 1f;
        private RectTransform progressRoot;

        private void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
            if (group != null) group.alpha = 0f;
            EnsureProgressBar();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (IsShowing) Time.timeScale = previousScale;
        }

        public void Show(BossIntroSO data, System.Action onComplete = null)
        {
            if (data == null) { onComplete?.Invoke(); return; }
            if (IsShowing) return;

            if (MenuManager.Instance != null) MenuManager.Instance.CloseCurrent();

            IsShowing = true;
            previousScale = Time.timeScale;
            Time.timeScale = 0f;

            if (background != null) background.color = data.Background;
            if (portrait != null)
            {
                portrait.sprite = data.Portrait;
                portrait.enabled = data.Portrait != null;
                portrait.transform.localScale = Vector3.one * data.PortraitScale;
            }
            if (title != null) { title.text = data.Title; title.color = data.Accent; }
            if (subtitle != null) subtitle.text = data.Subtitle;
            if (hint != null) hint.text = data.Hint;
            if (audioSource != null && data.Sfx != null) audioSource.PlayOneShot(data.Sfx);

            if (panel != null) panel.SetActive(true);
            EnsureProgressBar();
            SetProgress(0f);
            if (group != null)
            {
                group.alpha = 0f;
                Tween.Alpha(group, 1f, data.FadeIn, useUnscaledTime: true);
            }

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(WaitAndClose(data, onComplete));
        }

        private IEnumerator WaitAndClose(BossIntroSO data, System.Action onComplete)
        {
            try
            {
                float total = data.MaxWaitSeconds > 0f ? Mathf.Min(data.HoldDuration, data.MaxWaitSeconds) : data.HoldDuration;
                total = Mathf.Max(0.01f, total);
                float t = 0f;
                bool skipped = false;
                while (t < total && !skipped)
                {
                    t += Time.unscaledDeltaTime;
                    SetProgress(t / total);
                    if (data.WaitForInput && EnterPressed()) skipped = true;
                    yield return null;
                }
                SetProgress(1f);

                if (group != null) Tween.Alpha(group, 0f, data.FadeOut, useUnscaledTime: true);
                yield return new WaitForSecondsRealtime(data.FadeOut);
                if (panel != null) panel.SetActive(false);
            }
            finally
            {
                Time.timeScale = previousScale;
                IsShowing = false;
                routine = null;
            }

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
