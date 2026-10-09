using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Barra de vida del jefe: se muestra/oculta y se suscribe a los eventos de Health para reflejar la vida actual y máxima.
    /// </summary>
    public class BossHealthBarWidget : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image fill;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text hpLabel;
        [SerializeField] private Color fullColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private Color emptyColor = new Color(0.35f, 0.10f, 0.10f);

        private Health health;

        /// <summary>
        /// Inicializa la raíz (por defecto este GameObject) y oculta la barra al arrancar.
        /// </summary>
        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            Hide();
        }

        /// <summary>
        /// Muestra la barra para el jefe indicado y se suscribe a los cambios de vida de Health.
        /// </summary>
        public void Show(string bossName, Health target)
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

            if (nameLabel != null)
            {
                nameLabel.text = bossName;
            }

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        /// <summary>
        /// Deja de escuchar los cambios de vida y oculta la barra.
        /// </summary>
        public void Hide()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
                health = null;
            }

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        /// <summary>
        /// Deja de escuchar los cambios de vida del jefe al destruirse.
        /// </summary>
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

            if (hpLabel != null)
            {
                hpLabel.text = current + " / " + max;
            }
        }
    }
}
