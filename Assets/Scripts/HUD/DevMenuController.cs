using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Menú de desarrollo (debug) que expone en la UI controles de nivel, velocidad, daño, vida y desbloqueo de habilidades del jugador.
    /// </summary>
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
        private PlayerBuffs buffs;
        [SerializeField] private MerchantMenuUI merchantMenu;
        [SerializeField] private GameObject openMerchantButton;

        /// <summary>
        /// Enlaza los botones, configura los rangos de los sliders y oculta el panel al arrancar.
        /// </summary>
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
            if (openMerchantButton != null) openMerchantButton.SetActive(false);
        }

        /// <summary>
        /// Busca al jugador y enlaza los sliders y toggles con sus PlayerStats y PlayerAbilities.
        /// </summary>
        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            stats = player != null ? player.GetComponent<PlayerStats>() : null;
            abilities = player != null ? player.GetComponent<PlayerAbilities>() : null;
            buffs = player != null ? player.GetComponent<PlayerBuffs>() : null;

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

        /// <summary>
        /// Refresca la etiqueta de nivel y muestra el panel del menú.
        /// </summary>
        public void Open()
        {
            if (MenuManager.Instance != null) MenuManager.Instance.Open(this, Close);
            RefreshLevelLabel();
            if (panel != null) panel.SetActive(true);
            if (openMerchantButton != null) openMerchantButton.SetActive(true);
        }

        public void Close()
        {
            if (MenuManager.Instance != null) MenuManager.Instance.Release(this);
            if (panel != null) panel.SetActive(false);
            if (openMerchantButton != null) openMerchantButton.SetActive(false);
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

        /// <summary>Da fragmentos (moneda) para probar el comercio.</summary>
        public void GiveFragments(int amount)
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.AddFragments(amount);
            }
        }
        
        /// <summary>Restaura toda la vida.</summary>
        public void HealFull()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Health h = player != null ? player.GetComponent<Health>() : null;
            if (h != null)
            {
                h.SetMaxHealth(h.MaxHealth);
            }
        }
        
        /// <summary>Aplica mejora temporal de daño.</summary>
        public void BuffDamage()
        {
            if (buffs != null) buffs.ApplyBuff(BuffKind.Damage);
        }
        
        /// <summary>Aplica mejora temporal de velocidad.</summary>
        public void BuffSpeed()
        {
            if (buffs != null) buffs.ApplyBuff(BuffKind.Speed);
        }
        
        /// <summary>Aplica mejora temporal de vida máxima.</summary>
        public void BuffHealth()
        {
            if (buffs != null) buffs.ApplyBuff(BuffKind.MaxHealth);
        }
        /// <summary>Abre la tienda del comerciante (para pruebas).</summary>
        public void OpenMerchant()
        {
            if (merchantMenu == null) merchantMenu = FindAnyObjectByType<MerchantMenuUI>(FindObjectsInactive.Include);
            if (merchantMenu == null) return;
            merchantMenu.Open();
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
