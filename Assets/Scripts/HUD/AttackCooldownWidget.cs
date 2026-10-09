using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class AttackCooldownWidget : MonoBehaviour
    {
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Image meleeSweep;
        [SerializeField] private Image rangedSweep;
        [SerializeField] private Image meleeIcon;
        [SerializeField] private Image rangedIcon;
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color coolingColor = new Color(0.4f, 0.4f, 0.4f);

        public void Bind(PlayerCombat target)
        {
            combat = target;
            Refresh();
        }

        public void Refresh()
        {
            if (combat == null)
            {
                return;
            }

            float melee = combat.MeleeCooldownNormalized;
            float ranged = combat.RangedCooldownNormalized;

            if (meleeSweep != null) meleeSweep.fillAmount = melee;
            if (rangedSweep != null) rangedSweep.fillAmount = ranged;
            if (meleeIcon != null) meleeIcon.color = melee > 0f ? coolingColor : readyColor;
            if (rangedIcon != null) rangedIcon.color = ranged > 0f ? coolingColor : readyColor;
        }
    }
}
