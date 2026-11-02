using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Muestra páginas de historia entre mundos y avanza con ENTER o por tiempo.</summary>
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

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (group != null) group.alpha = 0f;
        }

        public void Play(StoryInterludeSO data, Action onComplete)
        {
            if (data == null || data.Pages == null || data.Pages.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(Run(data, onComplete));
        }

        private IEnumerator Run(StoryInterludeSO data, Action onComplete)
        {
            IsPlaying = true;
            if (audioSource != null && data.Music != null) audioSource.PlayOneShot(data.Music);
            if (panel != null) panel.SetActive(true);
            if (group != null) { group.alpha = 0f; Tween.Alpha(group, 1f, 0.3f, useUnscaledTime: true); }
            if (title != null) title.text = data.Title;

            for (int i = 0; i < data.Pages.Length; i++)
            {
                var page = data.Pages[i];
                if (image != null) { image.sprite = page.image; image.enabled = page.image != null; }
                if (body != null) body.text = page.text;
                if (pageLabel != null) pageLabel.text = (i + 1) + " / " + data.Pages.Length;
                if (hint != null) hint.text = data.Skippable ? "ENTER para continuar" : "";

                float t = 0f;
                while (t < page.holdSeconds && !(data.Skippable && EnterPressed()))
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                yield return null;
            }

            if (group != null) Tween.Alpha(group, 0f, 0.25f, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(0.3f);
            if (panel != null) panel.SetActive(false);
            IsPlaying = false;
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
