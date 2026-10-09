using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class DevMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button levelDownButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Text levelLabel;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private Slider damageSlider;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Toggle[] abilityToggles;

        private PlayerStats stats;
        private PlayerAbilities abilities;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (levelDownButton != null) levelDownButton.onClick.AddListener(() => ChangeLevel(-1));
            if (levelUpButton != null) levelUpButton.onClick.AddListener(() => ChangeLevel(1));

            if (speedSlider != null) { speedSlider.minValue = 0.5f; speedSlider.maxValue = 30f; }
            if (damageSlider != null) { damageSlider.minValue = 0.1f; damageSlider.maxValue = 10f; }
            if (healthSlider != null) { healthSlider.minValue = 1f; healthSlider.maxValue = 1000f; }

            if (panel != null) panel.SetActive(false);
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            stats = player != null ? player.GetComponent<PlayerStats>() : null;
            abilities = player != null ? player.GetComponent<PlayerAbilities>() : null;

            if (stats != null)
            {
                if (speedSlider != null)
                {
                    speedSlider.value = stats.BaseMoveSpeed;
                    speedSlider.onValueChanged.AddListener(stats.SetBaseMoveSpeed);
                }

                if (damageSlider != null)
                {
                    damageSlider.value = stats.BaseDamageMultiplier;
                    damageSlider.onValueChanged.AddListener(stats.SetBaseDamageMultiplier);
                }

                if (healthSlider != null)
                {
                    healthSlider.value = stats.BaseHealth;
                    healthSlider.onValueChanged.AddListener(stats.SetBaseHealth);
                }
            }

            if (abilityToggles != null)
            {
                for (int i = 0; i < abilityToggles.Length; i++)
                {
                    if (abilityToggles[i] == null)
                    {
                        continue;
                    }

                    int index = i;
                    if (abilities != null)
                    {
                        abilityToggles[i].isOn = !abilities.IsSlotLocked(index);
                    }

                    abilityToggles[i].onValueChanged.AddListener(value => OnAbilityToggled(index, value));
                }
            }

            RefreshLevelLabel();
        }

        public void Open()
        {
            RefreshLevelLabel();
            if (panel != null) panel.SetActive(true);
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void ChangeLevel(int delta)
        {
            if (stats == null)
            {
                return;
            }

            stats.SetLevel(stats.Level + delta);
            RefreshLevelLabel();
        }

        private void OnAbilityToggled(int index, bool enabled)
        {
            if (abilities != null)
            {
                abilities.SetSlotLocked(index, !enabled);
            }
        }

        private void RefreshLevelLabel()
        {
            if (levelLabel != null)
            {
                levelLabel.text = stats != null ? "Nivel: " + stats.Level + " / " + stats.MaxLevel : "Nivel: -";
            }
        }
    }
}
