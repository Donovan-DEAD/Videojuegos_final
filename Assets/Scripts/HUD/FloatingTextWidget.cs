using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Texto flotante '+X' bajo el contador de fragmentos (float + fade con PrimeTween).</summary>
    public class FloatingTextWidget : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private float riseDistance = 30f;
        [SerializeField] private float duration = 0.9f;

        private RectTransform rect;
        private Vector2 basePos;
        private Sequence sequence;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            if (label == null) label = GetComponent<Text>();
            basePos = rect != null ? rect.anchoredPosition : Vector2.zero;
            if (label != null) label.enabled = false;
        }

        private void Start()
        {
            if (GameFlowManager.Instance != null) GameFlowManager.Instance.FragmentsGained += OnGained;
        }

        private void OnDestroy()
        {
            if (GameFlowManager.Instance != null) GameFlowManager.Instance.FragmentsGained -= OnGained;
        }

        private void OnGained(int amount)
        {
            if (label == null) return;
            Show(amount);
        }

        private void Show(int amount)
        {
            if (sequence.isAlive) sequence.Stop();
            if (rect == null || label == null) return;

            label.text = "+" + amount;
            label.enabled = true;
            rect.anchoredPosition = basePos;
            label.color = new Color(label.color.r, label.color.g, label.color.b, 1f);

            sequence = Sequence.Create()
                .Chain(Tween.UIAnchoredPositionY(rect, basePos.y + riseDistance, duration))
                .Group(Tween.Alpha(label, 0f, duration))
                .ChainCallback(() =>
                {
                    label.enabled = false;
                    rect.anchoredPosition = basePos;
                });
        }
    }
}
