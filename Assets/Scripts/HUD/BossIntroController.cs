using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Muestra una intro a pantalla completa y pausa el juego mientras dura.</summary>
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

        private Coroutine routine;
        private float previousScale = 1f;

        private void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
            if (group != null) group.alpha = 0f;
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
            float t = 0f;
            bool skipped = false;
            while (t < data.HoldDuration && !skipped)
            {
                t += Time.unscaledDeltaTime;
                if (data.WaitForInput && EnterPressed()) skipped = true;
                yield return null;
            }

            if (group != null) Tween.Alpha(group, 0f, data.FadeOut, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(data.FadeOut);

            if (panel != null) panel.SetActive(false);
            Time.timeScale = previousScale;
            IsShowing = false;
            routine = null;
            onComplete?.Invoke();
        }

        private static bool EnterPressed()
        {
            Keyboard k = Keyboard.current;
            if (k != null) return k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame;
            try { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter); } catch { return false; }
        }
    }
}
