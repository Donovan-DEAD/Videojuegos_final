using System.Collections;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Gestiona los ataques del enemigo (melee o a distancia) con windup y enfriamiento,
    /// tomando rango, daño, cadencia y proyectil de EnemyStatsSO.
    /// </summary>
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask targetMask = ~0;

        private EnemyStatsSO stats;

        /// <summary>Rango de ataque efectivo según las estadísticas (con valor por defecto).</summary>
        public float AttackRange => stats != null ? stats.AttackRange : 1.5f;
        /// <summary>Tipo de ataque configurado (melee o a distancia).</summary>
        public AttackType AttackType => stats != null ? stats.AttackType : AttackType.Melee;
        public bool IsCoolingDown => cooldownRemaining > 0f;
        public bool IsWindingUp => windingUp;

        private float cooldownRemaining;
        private bool windingUp;

        /// <summary>Asigna las estadísticas usadas por los ataques.</summary>
        public void SetStats(EnemyStatsSO value)
        {
            stats = value;
        }

        /// <summary>Avanza el enfriamiento; lo invoca EnemyAI cada frame.</summary>
        public void Tick(float deltaTime)
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= deltaTime;
            }
        }

        /// <summary>Indica si el objetivo está dentro del rango de ataque.</summary>
        public bool IsInRange(Transform target)
        {
            if (stats == null || target == null)
            {
                return false;
            }

            return Vector3.Distance(transform.position, target.position) <= stats.AttackRange;
        }

        /// <summary>Indica si puede atacar ahora (en rango, sin enfriamiento ni windup).</summary>
        public bool CanAttack(Transform target)
        {
            return IsInRange(target) && !IsCoolingDown && !IsWindingUp;
        }

        /// <summary>Lanza el ataque (melee o a distancia) si es posible; devuelve si se inició.</summary>
        public bool TryAttack(IDamageable target, Transform targetTransform)
        {
            if (stats == null || target == null || targetTransform == null)
            {
                return false;
            }

            if (!CanAttack(targetTransform))
            {
                return false;
            }

            if (stats.AttackType == AttackType.Ranged)
            {
                StartCoroutine(RangedRoutine(targetTransform));
            }
            else
            {
                StartCoroutine(MeleeRoutine(target, targetTransform));
            }

            return true;
        }

        private IEnumerator MeleeRoutine(IDamageable target, Transform targetTransform)
        {
            windingUp = true;
            cooldownRemaining = stats.AttackCooldown;

            if (stats.WindupTime > 0f)
            {
                yield return new WaitForSeconds(stats.WindupTime);
            }

            windingUp = false;

            if (targetTransform == null)
            {
                yield break;
            }

            if (IsInRange(targetTransform))
            {
                target.TakeDamage(stats.Damage);
            }
        }

        private IEnumerator RangedRoutine(Transform targetTransform)
        {
            windingUp = true;
            cooldownRemaining = stats.AttackCooldown;

            if (stats.WindupTime > 0f)
            {
                yield return new WaitForSeconds(stats.WindupTime);
            }

            windingUp = false;

            if (targetTransform == null || stats.ProjectilePrefab == null)
            {
                yield break;
            }

            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            Vector2 direction = (Vector2)(targetTransform.position - origin);
            direction.Normalize();

            GameObject projectile = Instantiate(stats.ProjectilePrefab, origin, Quaternion.identity);
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Launch(direction, stats.Damage, stats.ProjectileSpeed, targetMask);
            }
        }
    }
}
