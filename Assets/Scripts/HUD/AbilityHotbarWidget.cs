using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Widget de la barra de habilidades: refleja el icono, el enfriamiento y el estado de bloqueo de cada ranura de PlayerAbilities.
    /// </summary>
    public class AbilityHotbarWidget : MonoBehaviour
    {
        [SerializeField] private Image[] slotIcons;
        [SerializeField] private Image[] slotSweeps;
        [SerializeField] private Text[] slotKeys;
        [SerializeField] private Image[] slotLocks;
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color lockedKeyColor = new Color(1f, 1f, 1f, 0.35f);

        private PlayerAbilities abilities;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            abilities = player != null ? player.GetComponent<PlayerAbilities>() : null;
        }

        private void LateUpdate()
        {
            if (abilities == null)
            {
                return;
            }

            int count = Mathf.Min(slotIcons != null ? slotIcons.Length : 0, abilities.SlotCount);
            for (int i = 0; i < count; i++)
            {
                BossAbilitySO ability = abilities.GetSlot(i);
                SetSlot(i, ability != null ? ability.Icon : null, abilities.GetCooldownNormalized(i), abilities.IsSlotLocked(i));
            }
        }

        /// <summary>
        /// Actualiza la apariencia de la ranura indicada: icono, barra de enfriamiento normalizada y estado de bloqueo.
        /// </summary>
        public void SetSlot(int index, Sprite icon, float cooldownNormalized, bool locked)
        {
            if (slotIcons == null || index < 0 || index >= slotIcons.Length)
            {
                return;
            }

            if (slotIcons[index] != null)
            {
                if (icon != null)
                {
                    slotIcons[index].sprite = icon;
                }

                slotIcons[index].color = locked ? new Color(0f, 0f, 0f, 0f) : readyColor;
            }

            if (slotSweeps != null && index < slotSweeps.Length && slotSweeps[index] != null)
            {
                slotSweeps[index].fillAmount = cooldownNormalized;
            }

            if (slotLocks != null && index < slotLocks.Length && slotLocks[index] != null)
            {
                slotLocks[index].gameObject.SetActive(locked);
            }

            if (slotKeys != null && index < slotKeys.Length && slotKeys[index] != null)
            {
                slotKeys[index].color = locked ? lockedKeyColor : Color.black;
            }
        }
    }
}
