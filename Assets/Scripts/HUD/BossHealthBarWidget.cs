using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class BossHealthBarWidget : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image fill;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text hpLabel;
        [SerializeField] private Color fullColor = new Color(0.85f, 0.25f, 0.25f);
        [SerializeField] private Color emptyColor = new Color(0.35f, 0.10f, 0.10f);

        private Health health;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            Hide();
        }

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
