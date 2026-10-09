using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Jefe "Caperucita": lobo que persigue al jugador y alterna entre ataques cuerpo a cuerpo,
    /// embestidas e invocación de lobos según su máquina de estados y sus fases de salud.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class CaperucitaBoss : MonoBehaviour, IBossController
    {
        /// <summary>
        /// Estados de la máquina de comportamiento del jefe Caperucita.
        /// </summary>
        private enum CaperucitaState
        {
            Stalk,
            Windup,
            Slash,
            Lunge,
            Recover,
            Summon,
            Dead
        }

        [Header("Stats")]
        [SerializeField] private BossStatsSO stats;

        [Header("Melee")]
        [SerializeField] private Transform slashPoint;
        [SerializeField] private LayerMask playerMask = ~0;

        [Header("Lunge")]
        [SerializeField] private float lungeSpeed = 14f;
        [SerializeField] private float lungeDuration = 0.22f;
        [SerializeField] private float lungeCooldown = 3.5f;
        [SerializeField] private float lungeDamageMultiplier = 1.5f;
        [SerializeField] private float lungeHitRadius = 1f;
        [SerializeField] private float lungeMinDistance = 2.5f;
        [SerializeField] private float lungeMaxDistance = 7f;

        [Header("Timing")]
        [SerializeField] private float recoverTime = 0.35f;
        [SerializeField] private float summonWindup = 0.7f;

        [Header("Summon")]
        [SerializeField] private GameObject[] minionPrefabs;
        [SerializeField] private int minionsPerSummon = 2;
        [SerializeField] private int extraWolvesOnEnrage = 2;
        [SerializeField] private float minionSpread = 2.5f;

        [Header("Enrage (last phase)")]
        [SerializeField] private float enrageSpeedMultiplier = 1.35f;
        [SerializeField] private float enrageCooldownMultiplier = 0.6f;
        [SerializeField] private float enrageLungeCooldownMultiplier = 0.5f;

        [Header("Loot")]
        [SerializeField] private GameObject lootPrefab;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string stateParameter = "State";

        private Rigidbody2D rb;
        private Health health;
        private Collider2D bodyCollider;
        private Transform player;

        private CaperucitaState state = CaperucitaState.Stalk;
        private int currentPhase;
        private float attackCooldownRemaining;
        private float lungeCooldownRemaining;
        private float stateTimer;
        private Vector2 lungeDirection;
        private bool lungeHit;

        public int CurrentPhase => currentPhase;

        private bool IsEnraged => currentPhase >= 2;

        private float ChaseSpeed => stats.ChaseSpeed * (IsEnraged ? enrageSpeedMultiplier : 1f);
        private float AttackCooldown => stats.AttackCooldown * (IsEnraged ? enrageCooldownMultiplier : 1f);
        private float LungeCooldown => lungeCooldown * (IsEnraged ? enrageLungeCooldownMultiplier : 1f);

        /// <summary>
        /// Almacena referencias al <see cref="Rigidbody2D"/>, la <see cref="Health"/> y el collider del cuerpo.
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            bodyCollider = GetComponent<Collider2D>();
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
            state = CaperucitaState.Stalk;
            ApplyStats();
        }

        private void ApplyStats()
        {
            if (stats == null)
            {
                Debug.LogWarning("CaperucitaBoss: no BossStatsSO assigned.", this);
                return;
            }

            health.SetMaxHealth(stats.MaxHealth);
        }

        private void Update()
        {
            if (state == CaperucitaState.Dead || stats == null)
            {
                return;
            }

            if (attackCooldownRemaining > 0f)
            {
                attackCooldownRemaining -= Time.deltaTime;
            }

            if (lungeCooldownRemaining > 0f)
            {
                lungeCooldownRemaining -= Time.deltaTime;
            }

            switch (state)
            {
                case CaperucitaState.Stalk: UpdateStalk(); break;
                case CaperucitaState.Windup: UpdateWindup(); break;
                case CaperucitaState.Slash: UpdateSlash(); break;
                case CaperucitaState.Lunge: UpdateLunge(); break;
                case CaperucitaState.Recover: UpdateRecover(); break;
                case CaperucitaState.Summon: UpdateSummon(); break;
            }

            UpdateAnimation();
        }

        private void UpdateStalk()
        {
            if (player == null)
            {
                StopMoving();
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > stats.AttackRange)
            {
                MoveTowards(player.position, ChaseSpeed);
            }
            else
            {
                StopMoving();
            }

            if (lungeCooldownRemaining <= 0f && distance >= lungeMinDistance && distance <= lungeMaxDistance)
            {
                StartLunge();
                return;
            }

            if (distance <= stats.AttackRange && attackCooldownRemaining <= 0f)
            {
                EnterState(CaperucitaState.Windup);
                stateTimer = stats.WindupTime;
            }
        }

        private void UpdateWindup()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                EnterState(CaperucitaState.Slash);
            }
        }

        private void UpdateSlash()
        {
            StopMoving();

            Vector3 origin = slashPoint != null ? slashPoint.position : transform.position;
            DealDamageInRadius(origin, stats.AttackRange, stats.Damage);

            attackCooldownRemaining = AttackCooldown;
            EnterState(CaperucitaState.Recover);
            stateTimer = recoverTime;
        }

        private void StartLunge()
        {
            if (player == null)
            {
                return;
            }

            lungeCooldownRemaining = LungeCooldown;
            lungeDirection = ((Vector2)(player.position - transform.position)).normalized;
            lungeHit = false;
            stateTimer = lungeDuration;
            EnterState(CaperucitaState.Lunge);
        }

        private void UpdateLunge()
        {
            rb.linearVelocity = lungeDirection * lungeSpeed;
            stateTimer -= Time.deltaTime;

            if (!lungeHit)
            {
                int damage = Mathf.RoundToInt(stats.Damage * lungeDamageMultiplier);
                lungeHit = DealDamageInRadius(transform.position, lungeHitRadius, damage);
            }

            if (stateTimer <= 0f)
            {
                StopMoving();
                attackCooldownRemaining = AttackCooldown;
                EnterState(CaperucitaState.Recover);
                stateTimer = recoverTime;
            }
        }

        private void UpdateRecover()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                EnterState(CaperucitaState.Stalk);
            }
        }

        private void UpdateSummon()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                SpawnWolves();
                EnterState(CaperucitaState.Stalk);
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            int newPhase = CalculatePhase(current, max);
            if (newPhase == currentPhase)
            {
                return;
            }

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
            Debug.Log($"{name} (Caperucita) entered phase {phase + 1}.", this);

            if (phase < 1)
            {
                return;
            }

            EnterState(CaperucitaState.Summon);
            stateTimer = summonWindup;
        }

        private void SpawnWolves()
        {
            if (minionPrefabs == null || minionPrefabs.Length == 0)
            {
                return;
            }

            int count = minionsPerSummon + (IsEnraged ? extraWolvesOnEnrage : 0);

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
                if (prefab == null)
                {
                    continue;
                }

                Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(minionSpread * 0.5f, minionSpread);
                Instantiate(prefab, transform.position + (Vector3)offset, Quaternion.identity);
            }
        }

        private bool DealDamageInRadius(Vector3 origin, float radius, int damage)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, playerMask);
            bool hitAny = false;

            for (int i = 0; i < hits.Length; i++)
            {
                IDamageable damageable = hits[i].GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = hits[i].GetComponentInParent<IDamageable>();
                }

                if (damageable == null || ReferenceEquals(damageable, health))
                {
                    continue;
                }

                damageable.TakeDamage(damage);
                hitAny = true;
            }

            return hitAny;
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            Vector2 direction = (Vector2)(target - transform.position);
            if (direction.sqrMagnitude < 0.0001f)
            {
                StopMoving();
                return;
            }

            rb.linearVelocity = direction.normalized * speed;
        }

        private void StopMoving()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void EnterState(CaperucitaState newState)
        {
            state = newState;
        }

        private void OnDied()
        {
            state = CaperucitaState.Dead;
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
                case CaperucitaState.Dead:
                    return AnimationState.Death;

                case CaperucitaState.Windup:
                case CaperucitaState.Slash:
                case CaperucitaState.Lunge:
                    return AnimationState.MeleeAttack;

                case CaperucitaState.Stalk:
                    return rb.linearVelocity.sqrMagnitude > 0.01f ? AnimationState.Move : AnimationState.Idle;

                default:
                    return AnimationState.Idle;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 slashOrigin = slashPoint != null ? slashPoint.position : transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slashOrigin, stats != null ? stats.AttackRange : 1.5f);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, lungeMaxDistance);
        }
    }
}
