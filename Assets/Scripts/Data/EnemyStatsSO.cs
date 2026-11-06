using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Hábitat del enemigo. Ground y Water pueden combinarse (anfibio); Flying permite
    /// atravesar muros y no se restringe por terreno.
    /// </summary>
    [System.Flags]
    public enum EnemyHabitat
    {
        None = 0,
        Ground = 1,
        Water = 2,
        Flying = 4
    }

    /// <summary>
    /// Estadísticas base de un enemigo: movimiento, salud, ataque, detección y terreno.
    /// Incluye también su identidad visual (nombre y color) para el greybox.
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

        [Header("Terrain")]
        [SerializeField] private EnemyHabitat habitat = EnemyHabitat.Ground;
        [SerializeField] private bool passThroughWalls;
        [SerializeField] private bool visionThroughWalls;

        [Header("Behavior")]
        [SerializeField] private bool hidesPeriodically;
        [SerializeField] private float hideIntervalMin = 5f;
        [SerializeField] private float hideIntervalMax = 8f;
        [SerializeField] private float hideDuration = 2f;
        [SerializeField] private TerrainType hideTerrain = TerrainType.Water;
        [SerializeField] private int hideSearchRadius = 10;
        [SerializeField] private float maxTimeOutOfWater;
        [SerializeField] private bool hideWhileNotVisible = true;
        [SerializeField] private bool stationary;
        [SerializeField] private bool immuneWhileIdle;

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

        /// <summary>Terreno en el que puede estar el enemigo (Ground, Water, Flying o combinación).</summary>
        public EnemyHabitat Habitat => habitat;

        /// <summary>True si el enemigo atraviesa la capa Walls; Flying lo implica.</summary>
        public bool PassThroughWalls => passThroughWalls || (habitat & EnemyHabitat.Flying) != 0;

        /// <summary>True si la línea de visión ignora los muros (apagado por defecto).</summary>
        public bool VisionThroughWalls => visionThroughWalls;

        public bool HidesPeriodically => hidesPeriodically;
        public float HideIntervalMin => Mathf.Max(0.5f, hideIntervalMin);
        public float HideIntervalMax => Mathf.Max(HideIntervalMin, hideIntervalMax);
        public float HideDuration => Mathf.Max(0.1f, hideDuration);
        public TerrainType HideTerrain => hideTerrain;
        public int HideSearchRadius => Mathf.Clamp(hideSearchRadius, 1, 30);
        public float MaxTimeOutOfWater => Mathf.Max(0f, maxTimeOutOfWater);
        public bool HideWhileNotVisible => hideWhileNotVisible;
        public bool Stationary => stationary;
        public bool ImmuneWhileIdle => immuneWhileIdle;

        /// <summary>Nombre visible del enemigo; si está vacío usa el nombre del asset.</summary>
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        /// <summary>Color identificador del arquetipo (borde del enemigo y etiqueta).</summary>
        public Color IdentityColor => identityColor;
    }
}
