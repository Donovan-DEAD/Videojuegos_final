using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Categoría visual de la habilidad en la hotbar.</summary>
    public enum AbilityCategory
    {
        Basic,
        Special
    }

    /// <summary>
    /// Identifica cada habilidad de jefe para elegir su comportamiento y efectos asociados.
    /// </summary>
    public enum BossAbilityType
    {
        /// <summary>Garra de lobo: ataque cuerpo a cuerpo de Caperucita.</summary>
        WolfClaw,
        /// <summary>Disparo de espinas: proyectil a distancia.</summary>
        ThornShot,
        /// <summary>Daga de Peter Pan: proyectil lanzado en abanico.</summary>
        PeterDagger,
        /// <summary>Polvo de hadas: efecto asociado a las hadas.</summary>
        FairyDust
    }

    /// <summary>
    /// Datos configurables de una habilidad de jefe: identidad, tiempos, daño, proyectil,
    /// ralentización y desplazamiento.
    /// </summary>
    [CreateAssetMenu(fileName = "Ability", menuName = "Madness Dreams/Boss Ability")]
    public class BossAbilitySO : ScriptableObject
    {
        [Header("Hotbar")]
        [SerializeField] private AbilityCategory category = AbilityCategory.Basic;

        [Header("Identity")]
        [SerializeField] private BossAbilityType type;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private BossIntroSO acquiredIntro;

        [Header("Timing")]
        [SerializeField] private float cooldown = 2f;
        [SerializeField] private float animationDuration = 0.3f;

        [Header("Damage")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float range = 2f;

        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private float spreadAngle = 0f;

        [Header("Slow")]
        [SerializeField] private float slowMultiplier = 1f;
        [SerializeField] private float slowDuration = 0f;

        [Header("Dash")]
        [SerializeField] private float dashDistance = 0f;
        [SerializeField] private float dashDuration = 0.2f;

        public AbilityCategory Category => category;
        public BossAbilityType Type => type;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;

        /// <summary>Pantalla mostrada al aprender la habilidad (overlay pausado).</summary>
        public BossIntroSO AcquiredIntro => acquiredIntro;
        public float Cooldown => cooldown;
        public float AnimationDuration => animationDuration;
        public int Damage => damage;
        public float Range => range;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public int ProjectileCount => projectileCount;
        public float SpreadAngle => spreadAngle;
        public float SlowMultiplier => slowMultiplier;
        public float SlowDuration => slowDuration;
        public float DashDistance => dashDistance;
        public float DashDuration => dashDuration;
    }
}
