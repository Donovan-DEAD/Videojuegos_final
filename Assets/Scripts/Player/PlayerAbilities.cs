using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MadnessDreams
{
    /// <summary>
    /// Gestiona las habilidades del jugador (BossAbilitySO) en ranuras con enfriamiento,
    /// permitiendo aprenderlas y lanzarlas con las teclas 1-4.
    /// </summary>
    public class PlayerAbilities : MonoBehaviour
    {
        [SerializeField] private List<BossAbilitySO> startingAbilities = new List<BossAbilitySO>();
        [SerializeField] private int maxAbilities = 4;
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private Transform castPoint;
        [SerializeField] private Transform meleePoint;

        private BossAbilitySO[] slots;
        private float[] cooldowns;
        private bool[] lockedSlots;
        private Rigidbody2D rb;
        private PlayerController controller;

        /// <summary>Número de ranuras de habilidad disponibles.</summary>
        public int SlotCount => slots != null ? slots.Length : 0;

        /// <summary>Cachea Rigidbody2D/PlayerController e inicializa las ranuras con las habilidades iniciales.</summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController>();

            slots = new BossAbilitySO[Mathf.Max(1, maxAbilities)];
            cooldowns = new float[slots.Length];
            lockedSlots = new bool[slots.Length];

            for (int i = 0; i < startingAbilities.Count && i < slots.Length; i++)
            {
                slots[i] = startingAbilities[i];
            }
        }

        private void Update()
        {
            for (int i = 0; i < cooldowns.Length; i++)
            {
                if (cooldowns[i] > 0f)
                {
                    cooldowns[i] -= Time.deltaTime;
                }
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (WasPressed(i))
                {
                    UseAbility(i);
                }
            }
        }

        /// <summary>Coloca una habilidad en la primera ranura libre; false si ya la tiene o no hay espacio.</summary>
        public bool Learn(BossAbilitySO ability)
        {
            if (ability == null || slots == null)
            {
                return false;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == ability)
                {
                    return false;
                }
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = ability;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Devuelve la habilidad de la ranura indicada, o null si el índice es inválido.</summary>
        public BossAbilitySO GetSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length)
            {
                return null;
            }

            return slots[index];
        }

        /// <summary>Indica si la ranura está bloqueada.</summary>
        public bool IsSlotLocked(int index)
        {
            return lockedSlots != null && index >= 0 && index < lockedSlots.Length && lockedSlots[index];
        }

        /// <summary>Bloquea o desbloquea una ranura.</summary>
        public void SetSlotLocked(int index, bool locked)
        {
            if (lockedSlots != null && index >= 0 && index < lockedSlots.Length)
            {
                lockedSlots[index] = locked;
            }
        }

        /// <summary>Devuelve el progreso de enfriamiento (0-1) de la ranura.</summary>
        public float GetCooldownNormalized(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length || slots[index] == null)
            {
                return 0f;
            }

            float cd = slots[index].Cooldown;
            return cd > 0f ? Mathf.Clamp01(cooldowns[index] / cd) : 0f;
        }

        private static bool WasPressed(int index)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                switch (index)
                {
                    case 0: return keyboard.digit1Key.wasPressedThisFrame;
                    case 1: return keyboard.digit2Key.wasPressedThisFrame;
                    case 2: return keyboard.digit3Key.wasPressedThisFrame;
                    case 3: return keyboard.digit4Key.wasPressedThisFrame;
                }
                return false;
            }

            switch (index)
            {
                case 0: return Input.GetKeyDown(KeyCode.Alpha1);
                case 1: return Input.GetKeyDown(KeyCode.Alpha2);
                case 2: return Input.GetKeyDown(KeyCode.Alpha3);
                case 3: return Input.GetKeyDown(KeyCode.Alpha4);
            }
            return false;
        }

        /// <summary>Lanza la habilidad de la ranura si no está bloqueada ni en enfriamiento.</summary>
        public bool UseAbility(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length || IsSlotLocked(index))
            {
                return false;
            }

            BossAbilitySO ability = slots[index];
            if (ability == null || cooldowns[index] > 0f)
            {
                return false;
            }

            cooldowns[index] = ability.Cooldown;
            Execute(ability);
            return true;
        }

        private void Execute(BossAbilitySO ability)
        {
            Vector2 aim = AimDirection();

            switch (ability.Type)
            {
                case BossAbilityType.WolfClaw:
                    if (ability.DashDistance > 0f)
                    {
                        StartCoroutine(Dash(aim, ability.DashDistance, ability.DashDuration));
                    }
                    MeleeHit(ability);
                    break;

                case BossAbilityType.FairyDust:
                    StartCoroutine(Dash(aim, ability.DashDistance, ability.DashDuration));
                    break;

                case BossAbilityType.ThornShot:
                case BossAbilityType.PeterDagger:
                    FireFan(ability, aim);
                    break;
            }
        }

        private Vector2 AimDirection()
        {
            Vector3 origin = castPoint != null ? castPoint.position : transform.position;
            Vector3 world;
            Mouse mouse = Mouse.current;

            if (Camera.main != null && mouse != null)
            {
                world = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            }
            else if (Camera.main != null)
            {
                world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                world = transform.position + transform.up;
            }

            Vector2 dir = (Vector2)(world - origin);
            return dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.up;
        }

        private void FireFan(BossAbilitySO ability, Vector2 aim)
        {
            if (ability.ProjectilePrefab == null)
            {
                return;
            }

            Vector3 origin = castPoint != null ? castPoint.position : transform.position;
            int count = Mathf.Max(1, ability.ProjectileCount);
            float spread = ability.SpreadAngle;
            float step = count > 1 ? spread / (count - 1) : 0f;
            float start = -spread * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angle = count > 1 ? start + step * i : 0f;
                Vector2 dir = Rotate(aim, angle);

                GameObject go = Instantiate(ability.ProjectilePrefab, origin, Quaternion.identity);
                Projectile projectile = go.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Launch(dir, ability.Damage, ability.ProjectileSpeed, enemyMask, ability.SlowMultiplier, ability.SlowDuration);
                }
            }
        }

        private void MeleeHit(BossAbilitySO ability)
        {
            Vector3 origin = meleePoint != null ? meleePoint.position : transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, ability.Range, enemyMask);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = hit.GetComponentInParent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(ability.Damage);
                }
            }
        }

        private IEnumerator Dash(Vector2 direction, float distance, float duration)
        {
            if (distance <= 0f || duration <= 0f)
            {
                yield break;
            }

            if (controller != null)
            {
                controller.ControlEnabled = false;
            }

            Vector2 velocity = direction * (distance / duration);
            float t = 0f;

            while (t < duration)
            {
                if (rb != null)
                {
                    rb.linearVelocity = velocity;
                }

                t += Time.deltaTime;
                yield return null;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (controller != null)
            {
                controller.ControlEnabled = true;
            }
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }
    }
}
