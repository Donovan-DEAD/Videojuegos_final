using UnityEngine;
using UnityEngine.InputSystem;

namespace MadnessDreams
{
    /// <summary>Activa pociones con las teclas 1-4.</summary>
    public class PotionController : MonoBehaviour
    {
        [SerializeField] private PotionInventory inventory;
        [SerializeField] private PlayerEffectTrail trail;
        [SerializeField] private Color healColor = new Color(0.35f, 1f, 0.45f);
        [SerializeField] private Color damageColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color speedColor = new Color(0.35f, 0.65f, 1f);
        [SerializeField] private Color healthColor = new Color(0.4f, 1f, 0.5f);

        private PlayerBuffs buffs;
        private Health health;

        private void Awake()
        {
            if (inventory == null) inventory = GetComponent<PotionInventory>();
            if (trail == null) trail = GetComponent<PlayerEffectTrail>();
            buffs = GetComponent<PlayerBuffs>();
            health = GetComponent<Health>();
        }

        private void Update()
        {
            if (inventory == null) return;
            if (Pressed(Key.Digit1, KeyCode.Alpha1)) Use(PotionKind.Heal);
            if (Pressed(Key.Digit2, KeyCode.Alpha2)) Use(PotionKind.Damage);
            if (Pressed(Key.Digit3, KeyCode.Alpha3)) Use(PotionKind.Speed);
            if (Pressed(Key.Digit4, KeyCode.Alpha4)) Use(PotionKind.MaxHealth);
        }

        private static bool Pressed(Key key, KeyCode old)
        {
            var k = Keyboard.current;
            if (k != null && k[key].wasPressedThisFrame) return true;
            try { return Input.GetKeyDown(old); } catch { return false; }
        }

        private void Use(PotionKind kind)
        {
            if (!inventory.TryConsume(kind)) return;
            switch (kind)
            {
                case PotionKind.Heal: if (health != null) health.SetMaxHealth(health.MaxHealth); break;
                case PotionKind.Damage: if (buffs != null) buffs.ApplyBuff(BuffKind.Damage); break;
                case PotionKind.Speed: if (buffs != null) buffs.ApplyBuff(BuffKind.Speed); break;
                case PotionKind.MaxHealth: if (buffs != null) buffs.ApplyBuff(BuffKind.MaxHealth); break;
            }
        }

        private Color ColorFor(PotionKind kind)
        {
            switch (kind)
            {
                case PotionKind.Heal: return healColor;
                case PotionKind.Damage: return damageColor;
                case PotionKind.Speed: return speedColor;
                default: return healthColor;
            }
        }
    }
}
