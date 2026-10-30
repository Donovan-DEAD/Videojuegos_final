using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Entrada de la barra de efectos: icono, cantidad y reloj radial que se oscurece.</summary>
    public class EffectsBarEntry : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text countLabel;
        [SerializeField] private Image radialFill;
        [SerializeField] private float darkenMax = 0.55f;

        private PlayerBuffs buffs;
        private BuffKind kind;
        private int lastStacks;
        private Color baseIconColor;

        public void Bind(Sprite sprite, int stacks, PlayerBuffs owner, BuffKind effectKind)
        {
            buffs = owner;
            kind = effectKind;

            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                baseIconColor = iconImage.color;
            }

            lastStacks = stacks;
            if (countLabel != null) countLabel.text = stacks.ToString();
        }

        public void Tick()
        {
            if (buffs == null) return;

            int stacks = buffs.GetStacks(kind);
            if (stacks != lastStacks)
            {
                lastStacks = stacks;
                if (countLabel != null) countLabel.text = stacks.ToString();
            }

            float t = buffs.GetTimerNormalized(kind);
            if (radialFill != null) radialFill.fillAmount = t;

            if (iconImage != null)
            {
                Color dark = baseIconColor * (1f - darkenMax);
                iconImage.color = Color.Lerp(dark, baseIconColor, t);
            }
        }
    }
}
