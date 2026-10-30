using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Entrada de la barra: icono, cantidad, tecla y reloj radial que se oscurece.</summary>
    public class EffectsBarEntry : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text countLabel;
        [SerializeField] private Text keyLabel;
        [SerializeField] private Image radialFill;
        [SerializeField] private float darkenMax = 0.55f;

        private PotionInventory inventory;
        private PlayerBuffs buffs;
        private PotionKind kind;
        private int lastCount;
        private Color baseIconColor;

        public void Bind(Sprite sprite, PotionInventory inv, PlayerBuffs owner, PotionKind effectKind)
        {
            inventory = inv;
            buffs = owner;
            kind = effectKind;

            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                baseIconColor = iconImage.color;
            }

            if (keyLabel != null) keyLabel.text = KeyOf(kind);

            lastCount = inventory != null ? inventory.Count(kind) : 0;
            if (countLabel != null) countLabel.text = lastCount.ToString();
        }

        public void Tick()
        {
            int count = inventory != null ? inventory.Count(kind) : 0;
            float timer = TimerNormalized();

            if (count != lastCount)
            {
                lastCount = count;
                if (countLabel != null) countLabel.text = count.ToString();
            }

            if (radialFill != null) radialFill.fillAmount = timer;

            if (keyLabel != null && keyLabel.transform.parent != null)
                keyLabel.transform.parent.gameObject.SetActive(KeyLabelsToggle.Visible);

            if (iconImage != null)
            {
                Color dark = baseIconColor * (1f - darkenMax);
                iconImage.color = Color.Lerp(dark, baseIconColor, timer);
            }

            if (count <= 0 && (kind == PotionKind.Heal || timer <= 0f)) gameObject.SetActive(false);
        }

        private float TimerNormalized()
        {
            if (kind == PotionKind.Heal || buffs == null) return 1f;

            BuffKind bk = kind == PotionKind.Damage ? BuffKind.Damage : (kind == PotionKind.Speed ? BuffKind.Speed : BuffKind.MaxHealth);
            return buffs.GetTimerNormalized(bk);
        }

        private static string KeyOf(PotionKind k)
        {
            switch (k)
            {
                case PotionKind.Heal: return "1";
                case PotionKind.Damage: return "2";
                case PotionKind.Speed: return "3";
                default: return "4";
            }
        }
    }
}
