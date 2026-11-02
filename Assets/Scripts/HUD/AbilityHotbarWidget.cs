using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Hotbar: icono, cooldown, bloqueo y marco por categoría; los slots vacíos se ven vacíos.</summary>
    public class AbilityHotbarWidget : MonoBehaviour
    {
        [SerializeField] private Image[] slotIcons;
        [SerializeField] private Image[] slotSweeps;
        [SerializeField] private Text[] slotKeys;
        [SerializeField] private Image[] slotLocks;
        [SerializeField] private Image[] slotFrames;
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.12f, 0.12f, 0.18f, 1f);
        [SerializeField] private Color lockedKeyColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color basicFrameColor = new Color(0.25f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color specialFrameColor = new Color(0.85f, 0.65f, 0.20f, 1f);
        [SerializeField] private Color emptyFrameColor = new Color(0.08f, 0.08f, 0.12f, 1f);

        private PlayerAbilities abilities;
        private Sprite[] defaultIcons;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            abilities = player != null ? player.GetComponent<PlayerAbilities>() : null;

            defaultIcons = new Sprite[slotIcons != null ? slotIcons.Length : 0];
            for (int i = 0; i < defaultIcons.Length; i++)
            {
                defaultIcons[i] = slotIcons[i] != null ? slotIcons[i].sprite : null;
            }

            ValidateArrays();
        }

        private void ValidateArrays()
        {
            if (slotIcons == null) return;
            int n = slotIcons.Length;
            if (slotSweeps != null && slotSweeps.Length != n) Debug.LogWarning("AbilityHotbarWidget: slotSweeps length != slotIcons", this);
            if (slotKeys != null && slotKeys.Length != n) Debug.LogWarning("AbilityHotbarWidget: slotKeys length != slotIcons", this);
            if (slotFrames != null && slotFrames.Length != n) Debug.LogWarning("AbilityHotbarWidget: slotFrames length != slotIcons", this);
        }

        private void LateUpdate()
        {
            if (abilities == null) return;

            int count = Mathf.Min(slotIcons != null ? slotIcons.Length : 0, abilities.SlotCount);
            for (int i = 0; i < count; i++)
            {
                BossAbilitySO ability = abilities.GetSlot(i);
                bool has = ability != null;
                Sprite icon = has ? (ability.Icon != null ? ability.Icon : (i < defaultIcons.Length ? defaultIcons[i] : null)) : null;

                SetSlot(i, icon, abilities.GetCooldownNormalized(i), abilities.IsSlotLocked(i),
                        ability != null ? ability.Category : AbilityCategory.Basic, has);
            }
        }

        /// <summary>Actualiza icono, cooldown, bloqueo, marco por categoría y estado vacío.</summary>
        public void SetSlot(int index, Sprite icon, float cooldownNormalized, bool locked, AbilityCategory category = AbilityCategory.Basic, bool hasAbility = true)
        {
            if (slotIcons == null || index < 0 || index >= slotIcons.Length) return;

            if (slotIcons[index] != null)
            {
                if (icon != null) slotIcons[index].sprite = icon;
                slotIcons[index].color = (hasAbility && !locked) ? readyColor : new Color(1f, 1f, 1f, 0f);
            }

            if (slotSweeps != null && index < slotSweeps.Length && slotSweeps[index] != null) slotSweeps[index].fillAmount = cooldownNormalized;

            if (slotLocks != null && index < slotLocks.Length && slotLocks[index] != null) slotLocks[index].gameObject.SetActive(locked || !hasAbility);

            if (slotFrames != null && index < slotFrames.Length && slotFrames[index] != null)
            {
                if (!hasAbility) slotFrames[index].color = emptyFrameColor;
                else slotFrames[index].color = category == AbilityCategory.Special ? specialFrameColor : basicFrameColor;
            }

            if (slotKeys != null && index < slotKeys.Length && slotKeys[index] != null) slotKeys[index].color = (hasAbility && !locked) ? Color.black : lockedKeyColor;
        }
    }
}
