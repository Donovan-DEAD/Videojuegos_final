using UnityEngine;

namespace MadnessDreams
{
    public enum BossAbilityType
    {
        WolfClaw,
        ThornShot,
        PeterDagger,
        FairyDust
    }

    [CreateAssetMenu(fileName = "Ability", menuName = "Madness Dreams/Boss Ability")]
    public class BossAbilitySO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private BossAbilityType type;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

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

        public BossAbilityType Type => type;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
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
