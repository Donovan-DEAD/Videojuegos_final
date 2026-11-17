using UnityEngine;
using UnityEngine.InputSystem;

namespace MadnessDreams
{
    /// <summary>
    /// Controla los ataques melee (barrido circular) y a distancia (proyectil) del jugador,
    /// con enfriamientos, multiplicador de daño y entrada por teclado.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Melee")]
        [SerializeField] private int meleeDamage = 10;
        [SerializeField] private float meleeRange = 1.5f;
        [SerializeField] private float meleeCooldown = 0.5f;
        [SerializeField] private float meleeAttackDuration = 0.3f;
        [SerializeField] private Transform meleePoint;
        [SerializeField] private MeleeRangeIndicator meleeIndicatorPrefab;

        [Header("Ranged")]
        [SerializeField] private int rangedDamage = 5;
        [SerializeField] private float rangedCooldown = 0.5f;
        [SerializeField] private float rangedAttackDuration = 0.3f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private Transform firePoint;

        [Header("Targets")]
        [SerializeField] private LayerMask enemyMask = ~0;

        public bool IsMeleeAttacking => meleeAttackTimer > 0f;
        public bool IsRangedAttacking => rangedAttackTimer > 0f;
        public bool IsAttacking => IsMeleeAttacking || IsRangedAttacking;

        public float MeleeCooldownNormalized => meleeCooldown > 0f ? Mathf.Clamp01(meleeCooldownRemaining / meleeCooldown) : 0f;
        public float RangedCooldownNormalized => rangedCooldown > 0f ? Mathf.Clamp01(rangedCooldownRemaining / rangedCooldown) : 0f;

        /// <summary>Ajusta el multiplicador de daño aplicado a los ataques.</summary>
        public void SetDamageMultiplier(float value)
        {
            damageMultiplier = Mathf.Max(0f, value);
        }

        private float damageMultiplier = 1f;

        private float meleeCooldownRemaining;
        private float rangedCooldownRemaining;
        private float meleeAttackTimer;
        private float rangedAttackTimer;

        private MeleeRangeIndicator meleeIndicator;

        private void Update()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }

            if (meleeCooldownRemaining > 0f) meleeCooldownRemaining -= Time.deltaTime;
            if (rangedCooldownRemaining > 0f) rangedCooldownRemaining -= Time.deltaTime;
            if (meleeAttackTimer > 0f) meleeAttackTimer -= Time.deltaTime;
            if (rangedAttackTimer > 0f) rangedAttackTimer -= Time.deltaTime;

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard != null && mouse != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) TryMeleeAttack();
                if (mouse.leftButton.wasPressedThisFrame) TryRangedAttack();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space)) TryMeleeAttack();
                if (Input.GetMouseButtonDown(0)) TryRangedAttack();
            }
        }

        /// <summary>Ejecuta un ataque melee si no está en enfriamiento, dañando a los IDamageable del área.</summary>
        public bool TryMeleeAttack()
        {
            if (meleeCooldownRemaining > 0f)
            {
                return false;
            }

            meleeCooldownRemaining = meleeCooldown;
            meleeAttackTimer = meleeAttackDuration;

            Vector3 origin = meleePoint != null ? meleePoint.position : transform.position;
            ShowMeleeIndicator(origin, meleeRange);
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, meleeRange, enemyMask);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = hit.GetComponentInParent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(Mathf.RoundToInt(meleeDamage * damageMultiplier));
                }
            }

            return hits.Length > 0;
        }

        /// <summary>Dispara un proyectil hacia el cursor si no está en enfriamiento.</summary>
        public bool TryRangedAttack()
        {
            if (rangedCooldownRemaining > 0f || projectilePrefab == null)
            {
                return false;
            }

            rangedCooldownRemaining = rangedCooldown;
            rangedAttackTimer = rangedAttackDuration;

            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            Mouse mouse = Mouse.current;
            Vector3 aimWorld = Camera.main != null && mouse != null
                ? Camera.main.ScreenToWorldPoint(mouse.position.ReadValue())
                : Camera.main != null
                    ? Camera.main.ScreenToWorldPoint(Input.mousePosition)
                    : transform.position + transform.up;

            Vector2 direction = (Vector2)(aimWorld - origin);
            direction.Normalize();

            // El firePoint está en el centro del sprite; adelantar el spawn 0.5u
            // para que el proyectil nazca por delante del personaje y no por detrás.
            Vector3 spawnAt = origin + (Vector3)direction * 0.5f;
            GameObject instance = Instantiate(projectilePrefab, spawnAt, Quaternion.identity);
            Projectile projectile = instance.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Launch(direction, Mathf.RoundToInt(rangedDamage * damageMultiplier), projectileSpeed, enemyMask);
            }

            return true;
        }

        /// <summary>
        /// Muestra el círculo del área melee, creando una única instancia reutilizable.
        /// </summary>
        private void ShowMeleeIndicator(Vector3 origin, float radius)
        {
            if (meleeIndicatorPrefab == null)
            {
                return;
            }

            if (meleeIndicator == null)
            {
                meleeIndicator = Instantiate(meleeIndicatorPrefab);
            }

            meleeIndicator.Show(origin, radius, meleeAttackDuration, meleePoint != null ? meleePoint : transform);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = meleePoint != null ? meleePoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, meleeRange);
        }
    }
}
