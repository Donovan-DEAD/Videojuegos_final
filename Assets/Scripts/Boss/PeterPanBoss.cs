using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Jefe "Peter Pan": enemigo volador que mantiene distancia, dispara andanadas de dagas,
    /// se reposiciona con vuelos e invoca esbirros al cambiar de fase.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class PeterPanBoss : MonoBehaviour, IBossController
    {
        /// <summary>
        /// Estados de la máquina de comportamiento del jefe Peter Pan.
        /// </summary>
        private enum PeterPanState
        {
            Kite,
            VolleyWindup,
            Reposition,
            Recover,
            Summon,
            Dead
        }

        [Header("Stats")]
        [SerializeField] private BossStatsSO stats;

        [Header("Volley")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask playerMask = ~0;
        [SerializeField] private int[] daggersPerPhase = new int[] { 1, 3, 5 };
        [SerializeField] private float spreadAngle = 30f;

        [Header("Kiting")]
        [SerializeField] private float preferredDistance = 6f;
        [SerializeField] private float closeRange = 3f;
        [SerializeField] private float strafeSpeed = 3f;
        [SerializeField] private float orbitFlipInterval = 1.2f;

        [Header("Reposition (flight)")]
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 4f;

        [Header("Timing")]
        [SerializeField] private float recoverTime = 0.4f;
        [SerializeField] private float summonWindup = 0.7f;

        [Header("Summon")]
        [SerializeField] private GameObject[] minionPrefabs;
        [SerializeField] private int minionsPerSummon = 2;
        [SerializeField] private int extraMinionsOnEnrage = 2;
        [SerializeField] private float minionSpread = 2.5f;
        [SerializeField] private int minionWaves = 3;
        [SerializeField] private float waveInterval = 1.2f;
        [SerializeField] private GameObject summonEffectPrefab;
        [Header("Character (Peter Pan)")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Color enrageColor = new Color(0.15f, 0.55f, 0.2f, 1f);
        [SerializeField] private float blinkDistance = 4f;

        [Header("Shadow Clone")]
        [SerializeField] private GameObject shadowClonePrefab;
        [SerializeField] private float shadowCooldown = 9f;
        [SerializeField] private float shadowDuration = 4f;
        private float shadowCooldownRemaining;
        private bool summoning;

        [Header("Enrage (last phase)")]
        [SerializeField] private float enrageSpeedMultiplier = 1.25f;
        [SerializeField] private float enrageCooldownMultiplier = 0.65f;
        [SerializeField] private float enrageDashCooldownMultiplier = 0.55f;
        [SerializeField] private float enrageSpreadMultiplier = 1.35f;

        [Header("Loot")]
        [SerializeField] private GameObject lootPrefab;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string stateParameter = "State";

        private Rigidbody2D rb;
        private Health health;
        private Collider2D bodyCollider;
        private Transform player;

        private PeterPanState state = PeterPanState.Kite;
        private int currentPhase;
        private float attackCooldownRemaining;
        private float dashCooldownRemaining;
        private float stateTimer;
        private float orbitTimer;
        private float orbitSign = 1f;
        private Vector2 dashDirection;

        public int CurrentPhase => currentPhase;

        private bool IsEnraged => currentPhase >= 2;

        private float ApproachSpeed => stats.ChaseSpeed * (IsEnraged ? enrageSpeedMultiplier : 1f);
        private float AttackCooldown => stats.AttackCooldown * (IsEnraged ? enrageCooldownMultiplier : 1f);
        private float DashCooldown => dashCooldown * (IsEnraged ? enrageDashCooldownMultiplier : 1f);
        private float CurrentSpreadAngle => spreadAngle * (IsEnraged ? enrageSpreadMultiplier : 1f);

        /// <summary>
        /// Almacena referencias al <see cref="Rigidbody2D"/>, la <see cref="Health"/> y el collider del cuerpo.
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            bodyCollider = GetComponent<Collider2D>();
            if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Suscribe los manejadores a los eventos de la salud y localiza al jugador si falta.
        /// </summary>
        private void Start()
        {
            health.Died += OnDied;
            health.HealthChanged += OnHealthChanged;

            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
            }
        }

        /// <summary>
        /// Se desuscribe de los eventos de la salud al destruirse el objeto.
        /// </summary>
        private void OnDestroy()
        {
            if (health == null)
            {
                return;
            }

            health.Died -= OnDied;
            health.HealthChanged -= OnHealthChanged;
        }

        /// <summary>
        /// Inyecta las estadísticas del jefe y el objetivo, reinicia fase y estado y aplica la salud máxima.
        /// </summary>
        public void ConfigureBoss(BossStatsSO bossStats, Transform target)
        {
            if (bossStats != null)
            {
                stats = bossStats;
            }

            if (target != null)
            {
                player = target;
            }

            currentPhase = 0;
            state = PeterPanState.Kite;
            ApplyStats();
        }

        private void ApplyStats()
        {
            if (stats == null)
            {
                Debug.LogWarning("PeterPanBoss: no BossStatsSO assigned.", this);
                return;
            }

            health.SetMaxHealth(stats.MaxHealth);
        }

        private void Update()
        {
            if (state == PeterPanState.Dead || stats == null)
            {
                return;
            }

            if (attackCooldownRemaining > 0f)
            {
                attackCooldownRemaining -= Time.deltaTime;
            }

            if (dashCooldownRemaining > 0f)
            {
                dashCooldownRemaining -= Time.deltaTime;
            }

            if (shadowCooldownRemaining > 0f)
            {
                shadowCooldownRemaining -= Time.deltaTime;
            }

            switch (state)
            {
                case PeterPanState.Kite: UpdateKite(); break;
                case PeterPanState.VolleyWindup: UpdateVolleyWindup(); break;
                case PeterPanState.Reposition: UpdateReposition(); break;
                case PeterPanState.Recover: UpdateRecover(); break;
                case PeterPanState.Summon: UpdateSummon(); break;
            }

            UpdateAnimation();
        }

        private void UpdateKite()
        {
            if (player == null)
            {
                StopMoving();
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            Vector2 toPlayer = ((Vector2)(player.position - transform.position)).normalized;

            if (distance < closeRange && dashCooldownRemaining <= 0f)
            {
                if (shadowClonePrefab != null && shadowCooldownRemaining <= 0f)
                {
                    SpawnShadowClone();
                    shadowCooldownRemaining = shadowCooldown;
                }

                Blink(-toPlayer);
                return;
            }

            if (distance < closeRange)
            {
                rb.linearVelocity = -toPlayer * strafeSpeed;
            }
            else if (distance > preferredDistance)
            {
                rb.linearVelocity = toPlayer * ApproachSpeed;
            }
            else
            {
                orbitTimer -= Time.deltaTime;
                if (orbitTimer <= 0f)
                {
                    orbitTimer = orbitFlipInterval;
                    orbitSign = -orbitSign;
                }

                Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x) * orbitSign;
                rb.linearVelocity = tangent * strafeSpeed;
            }

            if (distance <= stats.AttackRange && attackCooldownRemaining <= 0f)
            {
                StopMoving();
                EnterState(PeterPanState.VolleyWindup);
                stateTimer = stats.WindupTime;
            }
        }

        private void UpdateVolleyWindup()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                FireVolley();
            }
        }

        private void SpawnShadowClone()
        {
            if (shadowClonePrefab == null) return;

            GameObject clone = Instantiate(shadowClonePrefab, transform.position, Quaternion.identity);
            ShadowClone sc = clone.GetComponent<ShadowClone>();
            if (sc != null) sc.Setup(player, transform.position, shadowDuration);
        }

        private void Blink(Vector2 direction)
        {
            if (summonEffectPrefab != null) Instantiate(summonEffectPrefab, transform.position, Quaternion.identity);

            Vector3 target = transform.position + (Vector3)(direction.normalized * blinkDistance);
            transform.position = target;

            if (summonEffectPrefab != null) Instantiate(summonEffectPrefab, transform.position, Quaternion.identity);
            dashCooldownRemaining = DashCooldown;
            EnterState(PeterPanState.Kite);
        }

        private void StartReposition(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Random.insideUnitCircle.normalized;
            }

            dashDirection = direction.normalized;
            dashCooldownRemaining = DashCooldown;
            stateTimer = dashDuration;
            EnterState(PeterPanState.Reposition);
        }

        private void UpdateReposition()
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                StopMoving();
                EnterState(PeterPanState.Kite);
            }
        }

        private void UpdateRecover()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                EnterState(PeterPanState.Kite);
            }
        }

        private void UpdateSummon()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f && !summoning)
            {
                StartCoroutine(SummonWaves());
            }
        }

        private void FireVolley()
        {
            GameObject prefab = ResolveProjectilePrefab();
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;

            if (prefab != null && player != null)
            {
                Vector2 baseDirection = ((Vector2)(player.position - origin)).normalized;
                int count = DaggersForPhase(currentPhase);
                float spread = CurrentSpreadAngle;
                float step = count > 1 ? spread / (count - 1) : 0f;
                float start = -spread * 0.5f;

                for (int i = 0; i < count; i++)
                {
                    float angle = count > 1 ? start + step * i : 0f;
                    Vector2 direction = Rotate(baseDirection, angle);

                    GameObject instance = Instantiate(prefab, origin, Quaternion.identity);
                    Projectile projectile = instance.GetComponent<Projectile>();
                    if (projectile != null)
                    {
                        projectile.Launch(direction, stats.Damage, stats.ProjectileSpeed, playerMask);
                    }
                }
            }

            attackCooldownRemaining = AttackCooldown;
            EnterState(PeterPanState.Recover);
            stateTimer = recoverTime;
        }

        private GameObject ResolveProjectilePrefab()
        {
            return projectilePrefab != null ? projectilePrefab : stats.ProjectilePrefab;
        }

        private int DaggersForPhase(int phase)
        {
            if (daggersPerPhase == null || daggersPerPhase.Length == 0)
            {
                return 1;
            }

            return Mathf.Max(1, daggersPerPhase[Mathf.Clamp(phase, 0, daggersPerPhase.Length - 1)]);
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        private void OnHealthChanged(int current, int max)
        {
            int newPhase = CalculatePhase(current, max);
            if (newPhase == currentPhase) return;
            if (newPhase < currentPhase) { currentPhase = newPhase; return; }

            currentPhase = newPhase;
            OnPhaseChanged(currentPhase);
        }

        private int CalculatePhase(int currentHealth, int maxHealth)
        {
            if (stats == null || stats.PhaseHealthThresholds == null)
            {
                return 0;
            }

            float ratio = (float)currentHealth / Mathf.Max(1, maxHealth);
            float[] thresholds = stats.PhaseHealthThresholds;

            int phase = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (ratio <= thresholds[i])
                {
                    phase = i + 1;
                }
            }

            return phase;
        }

        private void OnPhaseChanged(int phase)
        {
            Debug.Log($"{name} (Peter Pan) entered phase {phase + 1}.", this);

            if (bodyRenderer != null && phase >= 2) bodyRenderer.color = enrageColor;

            if (phase < 1)
            {
                return;
            }

            EnterState(PeterPanState.Summon);
            stateTimer = summonWindup;
        }

        private System.Collections.IEnumerator SummonWaves()
        {
            summoning = true;
            int waves = minionWaves + (IsEnraged ? 1 : 0);
            for (int w = 0; w < waves; w++)
            {
                if (state == PeterPanState.Dead) break;
                if (summonEffectPrefab != null) Instantiate(summonEffectPrefab, transform.position, Quaternion.identity);
                SpawnMinions();
                yield return new WaitForSeconds(waveInterval);
            }
            summoning = false;
            if (state != PeterPanState.Dead) EnterState(PeterPanState.Kite);
        }

        private void SpawnMinions()
        {
            if (minionPrefabs == null || minionPrefabs.Length == 0)
            {
                return;
            }

            int count = minionsPerSummon + (IsEnraged ? extraMinionsOnEnrage : 0);

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
                if (prefab == null)
                {
                    continue;
                }

                Vector3 spawnPos = TerrainSpawnUtility.ResolveSpawnPosition(
                    prefab, transform.position, minionSpread * 0.5f, minionSpread);
                Instantiate(prefab, spawnPos, Quaternion.identity);
                if (summonEffectPrefab != null)
                {
                    GameObject fx = Instantiate(summonEffectPrefab, spawnPos, Quaternion.identity);
                    fx.transform.localScale *= 0.6f;
                }
            }
        }

        private void StopMoving()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void EnterState(PeterPanState newState)
        {
            state = newState;
        }

        private void OnDied()
        {
            state = PeterPanState.Dead;
            StopMoving();

            if (bodyCollider != null)
            {
                bodyCollider.enabled = false;
            }

            if (lootPrefab != null)
            {
                Instantiate(lootPrefab, transform.position, Quaternion.identity);
            }

            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(stateParameter, (int)DetermineAnimationState());
        }

        private AnimationState DetermineAnimationState()
        {
            switch (state)
            {
                case PeterPanState.Dead:
                    return AnimationState.Death;

                case PeterPanState.VolleyWindup:
                    return AnimationState.RangedAttack;

                case PeterPanState.Kite:
                case PeterPanState.Reposition:
                    return rb.linearVelocity.sqrMagnitude > 0.01f ? AnimationState.Move : AnimationState.Idle;

                default:
                    return AnimationState.Idle;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, preferredDistance);

            Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, closeRange);

            if (stats != null)
            {
                Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
                Gizmos.DrawWireSphere(transform.position, stats.AttackRange);
            }
        }
    }
}
