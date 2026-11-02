using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Datos reutilizables de la pantalla de introducción de un boss o miniboss.</summary>
    [CreateAssetMenu(fileName = "BossIntro", menuName = "Madness Dreams/Boss Intro")]
    public class BossIntroSO : ScriptableObject
    {
        [Header("Visual")]
        [SerializeField] private Sprite portrait;
        [SerializeField] private Color accent = Color.white;
        [SerializeField] private Color background = new Color(0f, 0f, 0f, 0.85f);
        [SerializeField] private float portraitScale = 1f;

        [Header("Text")]
        [SerializeField] private string title;
        [SerializeField, TextArea] private string subtitle;
        [SerializeField] private string hint = "Presiona ENTER para continuar";

        [Header("Timing")]
        [SerializeField] private float fadeIn = 0.35f;
        [SerializeField] private float fadeOut = 0.25f;
        [SerializeField] private float holdDuration = 999f;
        [SerializeField] private bool waitForInput = true;
        [SerializeField] private float maxWaitSeconds = 15f;

        [Header("Audio (opcional)")]
        [SerializeField] private AudioClip sfx;

        public Sprite Portrait => portrait;
        public Color Accent => accent;
        public Color Background => background;
        public float PortraitScale => portraitScale;
        public string Title => title;
        public string Subtitle => subtitle;
        public string Hint => hint;
        public float FadeIn => fadeIn;
        public float FadeOut => fadeOut;
        public float HoldDuration => holdDuration;
        public bool WaitForInput => waitForInput;
        public float MaxWaitSeconds => maxWaitSeconds;
        public AudioClip Sfx => sfx;
    }
}
