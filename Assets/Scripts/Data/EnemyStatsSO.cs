using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Estadísticas base de un enemigo: movimiento, salud, ataque y detección.
    /// Incluye también su identidad visual (nombre e color) para el greybox.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Madness Dreams/Enemy Stats")]
    public class EnemyStatsSO : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float chaseSpeed = 3.5f;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;

        [Header("Attack")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float windupTime = 0.3f;
        [SerializeField] private AttackType attackType = AttackType.Melee;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 8f;

        [Header("Detection")]
        [SerializeField] private float detectionRadius = 8f;
        [SerializeField] private float fovRadius = 6f;
        [SerializeField] private float fovAngle = 120f;
        [SerializeField] private float hearingRadius = 4f;

        [Header("Identity (greybox)")]
        [SerializeField] private string displayName = "";
        [SerializeField] private Color identityColor = Color.white;

        public float MoveSpeed => moveSpeed;
        public float ChaseSpeed => chaseSpeed;
        public int MaxHealth => maxHealth;
        public int Damage => damage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float WindupTime => windupTime;
        public AttackType AttackType => attackType;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float DetectionRadius => detectionRadius;
        public float FovRadius => fovRadius;
        public float FovAngle => fovAngle;
        public float HearingRadius => hearingRadius;

        /// <summary>Nombre visible del enemigo; si está vacío usa el nombre del asset.</summary>
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        /// <summary>Color identificador del arquetipo (borde del enemigo y etiqueta).</summary>
        public Color IdentityColor => identityColor;
    }
}
