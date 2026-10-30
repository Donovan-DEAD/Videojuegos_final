using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Contador de fragmentos (moneda) con punch al ganar.</summary>
    public class TaleFragmentWidget : MonoBehaviour
    {
        [SerializeField] private Text countLabel;

        private RectTransform rect;

        private void Start()
        {
            rect = GetComponent<RectTransform>();

            if (GameFlowManager.Instance != null)
            {
                SetCount(GameFlowManager.Instance.Fragments);
                GameFlowManager.Instance.FragmentsChanged += SetCount;
                GameFlowManager.Instance.FragmentsGained += OnGained;
            }
        }

        private void OnDestroy()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.FragmentsChanged -= SetCount;
                GameFlowManager.Instance.FragmentsGained -= OnGained;
            }
        }

        public void SetCount(int count)
        {
            if (countLabel != null) countLabel.text = count.ToString();
        }

        private void OnGained(int amount)
        {
            if (rect != null) Tween.PunchScale(rect, Vector3.one * 0.25f, 0.3f);
        }
    }
}
