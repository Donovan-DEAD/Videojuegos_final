using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Barra de vida genérica que se suscribe a un Health y refleja la vida actual y máxima.
    /// </summary>
    public class HealthBarWidget : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Text label;
        [SerializeField] private Color fullColor = new Color(0.30f, 0.85f, 0.35f);
        [SerializeField] private Color emptyColor = new Color(0.85f, 0.25f, 0.25f);

        private Health health;

        /// <summary>
        /// Asocia la barra a un Health y se suscribe a sus cambios de vida.
        /// </summary>
        public void Bind(Health target)
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
            }

            health = target;

            if (health != null)
            {
                health.HealthChanged += OnHealthChanged;
                OnHealthChanged(health.CurrentHealth, health.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (fill != null)
            {
                fill.fillAmount = ratio;
                fill.color = Color.Lerp(emptyColor, fullColor, ratio);
            }

            if (label != null)
            {
                label.text = current + " / " + max;
            }
        }
    }
}
