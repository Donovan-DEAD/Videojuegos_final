using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Barra de vida con animación suave, ghost bar de daño y punch al recibir golpes.</summary>
    public class HealthBarWidget : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Image ghostFill;
        [SerializeField] private Text label;
        [SerializeField] private RectTransform punchTarget;
        [SerializeField] private Color fullColor = new Color(0.30f, 0.85f, 0.35f);
        [SerializeField] private Color emptyColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private float fillDuration = 0.35f;

        private Health health;
        private int lastHealth;

        public void Bind(Health target)
        {
            if (health != null) health.HealthChanged -= OnHealthChanged;
            health = target;

            if (health != null)
            {
                lastHealth = health.CurrentHealth;
                health.HealthChanged += OnHealthChanged;
                OnHealthChanged(health.CurrentHealth, health.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (health != null) health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            Color targetColor = Color.Lerp(emptyColor, fullColor, ratio);

            if (fill != null)
            {
                Tween.Custom(fill, fill.fillAmount, ratio, fillDuration, (img, v) => img.fillAmount = v, Ease.InOutQuad);
                Tween.Color(fill, targetColor, fillDuration, Ease.InOutQuad);
            }

            if (ghostFill != null)
            {
                if (ratio < ghostFill.fillAmount)
                {
                    Tween.Custom(ghostFill, ghostFill.fillAmount, ratio, fillDuration * 1.8f, (img, v) => img.fillAmount = v, Ease.InOutQuad);
                }
                else
                {
                    ghostFill.fillAmount = ratio;
                }
            }

            if (label != null) label.text = current + " / " + max;

            if (current < lastHealth)
            {
                RectTransform target = punchTarget != null ? punchTarget : (fill != null ? fill.rectTransform : null);
                if (target != null) Tween.PunchScale(target, Vector3.one * 0.06f, 0.3f);
            }

            lastHealth = current;
        }
    }
}
