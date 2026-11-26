using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Estados de la máquina de estados finitos (FSM) del enemigo.
    /// </summary>
    public enum EnemyState
    {
        /// <summary>Deambula y vigila la posición inicial.</summary>
        Patrol,
        /// <summary>Detectó al jugador; pausa antes de perseguir.</summary>
        Alert,
        /// <summary>Persigue al objetivo hasta entrar en rango o perderlo.</summary>
        Chase,
        /// <summary>Ataca al objetivo cuando está en rango.</summary>
        Attack,
        /// <summary>Regresa a su posición inicial tras perder al jugador.</summary>
        Return,
        /// <summary>Se esconde en su terreno y permanece quieto un tiempo.</summary>
        Hide,
        /// <summary>Muerto; detiene el movimiento y desactiva colisionadores.</summary>
        Dead
    }

    /// <summary>
    /// IA del enemigo mediante una FSM (Patrol/Alert/Chase/Attack/Return/Dead); coordina EnemyMovement,
    /// EnemyAttack, EnemyFOV y Health, y refleja el estado en el Animator (int "State").
    /// </summary>
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyAttack))]
    [RequireComponent(typeof(EnemyFOV))]
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [SerializeField] protected EnemyStatsSO stats;
        [SerializeField] private Animator animator;
        [SerializeField] private string stateParameter = "State";
        [SerializeField] private float fovCheckInterval = 0.1f;
        [SerializeField] private float alertTime = 0.3f;
        [SerializeField] private float lostPlayerTimeout = 3f;
        [SerializeField] private float deathLingerTime = 2f;
        [SerializeField] private int fragmentsPerKill = 1;
        [SerializeField] private GameObject fragmentPickupPrefab;
        [SerializeField] private bool showIdentityLabel;

        /// <summary>Estado actual de la FSM.</summary>
        public EnemyState State { get; private set; } = EnemyState.Patrol;
        /// <summary>Estadísticas activas (las usa el spawner de terreno al invocar).</summary>
        public EnemyStatsSO Stats => stats;
        /// <summary>Se dispara al morir el enemigo, antes de destruirse.</summary>
        public event System.Action<EnemyAI> Died;

        protected EnemyMovement movement;
        protected EnemyAttack attack;
        protected EnemyFOV fov;
        protected Health health;
        protected Transform player;
        protected IDamageable playerDamageable;

        private EnemyIdentity identity;
        private Vector3 homePosition;
        private float fovTimer;
        private bool canSeePlayer;
        private float alertTimer;
        private float lostTimer;
        private Vector2 patrolDirection;
        private float patrolTimer;
        private bool patrolIsMoving;
        private float blockedTimer;
        private float outOfWaterTimer;
        private float hideTimer;
        private float hideRemaining;
        private Vector3 hideTarget;
        private bool hideTargetSet;

        /// <summary>Cachea los componentes requeridos, guarda la posición inicial y aplica las estadísticas.</summary>
        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyAttack>();
            fov = GetComponent<EnemyFOV>();
            health = GetComponent<Health>();

            homePosition = transform.position;

            identity = showIdentityLabel ? GetComponent<EnemyIdentity>() : null;

            ApplyStats();
        }

        /// <summary>Busca al jugador por etiqueta si no hay objetivo y se suscribe a Health.Died.</summary>
        protected virtual void Start()
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
                if (playerObject != null)
                {
                    SetTarget(playerObject.transform);
                }
            }

            health.Died += OnDied;
        }

        /// <summary>Se desuscribe de Health.Died.</summary>
        protected virtual void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        /// <summary>Inyecta estadísticas y objetivo, y las aplica a los componentes dependientes.</summary>
        public void Configure(EnemyStatsSO newStats, Transform target)
        {
            stats = newStats;
            SetTarget(target);
            ApplyStats();
        }

        /// <summary>Fija el Transform objetivo y cachea su IDamageable.</summary>
        public void SetTarget(Transform target)
        {
            player = target;
            playerDamageable = target != null ? target.GetComponent<IDamageable>() : null;
        }

        /// <summary>Ejecuta el tick de la FSM; se omite cuando el enemigo está muerto.</summary>
        protected virtual void Update()
        {
            if (State == EnemyState.Dead)
            {
                return;
            }

            attack.Tick(Time.deltaTime);
            UpdatePerception();
            UpdateHideTimer();

            switch (State)
            {
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Alert: UpdateAlert(); break;
                case EnemyState.Chase: UpdateChase(); break;
                case EnemyState.Attack: UpdateAttack(); break;
                case EnemyState.Return: UpdateReturn(); break;
                case EnemyState.Hide: UpdateHide(); break;
            }

            UpdateAnimation();
        }

        private void ApplyStats()
        {
            if (stats == null)
            {
                return;
            }

            attack.SetStats(stats);
            fov.SetStats(stats);
            movement.SetHabitat(stats.Habitat);
            movement.SetStationary(stats.Stationary);
            ApplyColliderExclusions(stats.PassThroughWalls);
            hideTimer = stats.HidesPeriodically ? Random.Range(stats.HideIntervalMin, stats.HideIntervalMax) : float.MaxValue;
            health.SetMaxHealth(stats.MaxHealth);
            health.Invulnerable = stats.ImmuneWhileIdle;

            if (identity != null)
            {
                identity.Apply(stats);
            }
        }

        private void ApplyColliderExclusions(bool passThroughWalls)
        {
            int walls = LayerMask.GetMask("Walls");
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                LayerMask mask = colliders[i].excludeLayers;
                int value = mask.value;
                if (passThroughWalls)
                {
                    value |= walls;
                }
                else
                {
                    value &= ~walls;
                }

                mask.value = value;
                colliders[i].excludeLayers = mask;
            }
        }

        private void UpdatePerception()
        {
            fovTimer -= Time.deltaTime;
            if (fovTimer > 0f)
            {
                return;
            }

            fovTimer = fovCheckInterval;
            canSeePlayer = player != null && (fov.CanSeePlayer(player) || fov.CanHearPlayer(player));
        }

        private void UpdatePatrol()
        {
            if (UpdateOutOfWaterTimer())
            {
                return;
            }

            if (canSeePlayer)
            {
                EnterAlert();
                return;
            }

            if (stats != null && stats.Stationary)
            {
                movement.Stop();
                return;
            }

            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                patrolIsMoving = !patrolIsMoving;

                if (patrolIsMoving)
                {
                    patrolTimer = UnityEngine.Random.Range(1f, 3f);
                    patrolDirection = UnityEngine.Random.insideUnitCircle;
                }
                else
                {
                    patrolTimer = UnityEngine.Random.Range(0.5f, 2f);
                }
            }

            if (patrolIsMoving)
            {
                float speed = stats != null ? stats.MoveSpeed : 0f;
                movement.MoveDirection(patrolDirection, speed);
            }
            else
            {
                movement.Stop();
            }
        }

        private void UpdateAlert()
        {
            movement.Stop();
            alertTimer -= Time.deltaTime;

            if (alertTimer <= 0f)
            {
                ChangeState(EnemyState.Chase);
            }
        }

        private void UpdateChase()
        {
            if (UpdateOutOfWaterTimer())
            {
                return;
            }

            if (player == null)
            {
                ChangeState(EnemyState.Return);
                return;
            }

            if (attack.IsInRange(player))
            {
                ChangeState(EnemyState.Attack);
                return;
            }

            float leash = stats != null ? stats.DetectionRadius : 8f;
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > leash)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= lostPlayerTimeout)
                {
                    ChangeState(EnemyState.Return);
                    return;
                }
            }
            else
            {
                lostTimer = 0f;
            }

            if (stats != null && stats.Stationary)
            {
                movement.Stop();
                return;
            }

            float speed = stats != null ? stats.ChaseSpeed : 0f;
            movement.MoveTowards(player.position, speed);

            if (movement.LastMoveBlocked)
            {
                blockedTimer += Time.deltaTime;
                if (blockedTimer >= 1.5f)
                {
                    blockedTimer = 0f;
                    ChangeState(EnemyState.Return);
                }
            }
            else
            {
                blockedTimer = 0f;
            }
        }

        private void UpdateAttack()
        {
            if (UpdateOutOfWaterTimer())
            {
                return;
            }

            if (player == null)
            {
                ChangeState(EnemyState.Return);
                return;
            }

            if (stats != null && (stats.Habitat & EnemyHabitat.Flying) != 0 && movement.IsOnWallCell())
            {
                movement.MoveTowards(player.position, stats.ChaseSpeed);
                return;
            }

            movement.Stop();

            if (!attack.IsInRange(player))
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            attack.TryAttack(playerDamageable, player);
        }

        private void UpdateReturn()
        {
            if (canSeePlayer)
            {
                EnterAlert();
                return;
            }

            if (stats != null && stats.Stationary)
            {
                movement.Stop();
                ChangeState(EnemyState.Patrol);
                return;
            }

            float speed = stats != null ? stats.MoveSpeed : 0f;
            movement.MoveTowards(homePosition, speed);

            if (Vector3.Distance(transform.position, homePosition) <= 0.2f)
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        private void UpdateHideTimer()
        {
            if (stats == null || !stats.HidesPeriodically || State == EnemyState.Hide || State == EnemyState.Dead)
            {
                return;
            }

            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                EnterHide();
            }
        }

        private bool UpdateOutOfWaterTimer()
        {
            if (stats == null || stats.MaxTimeOutOfWater <= 0f)
            {
                return false;
            }

            if (movement.IsOnWater)
            {
                outOfWaterTimer = 0f;
                return false;
            }

            outOfWaterTimer += Time.deltaTime;
            if (outOfWaterTimer >= stats.MaxTimeOutOfWater)
            {
                EnterHide();
                return true;
            }

            return false;
        }

        private void EnterHide()
        {
            if (State == EnemyState.Dead)
            {
                return;
            }

            hideRemaining = stats != null ? stats.HideDuration : 2f;
            hideTargetSet = stats != null && stats.HideTerrain != TerrainType.Void
                && TerrainSpawnUtility.FindNearestTerrain(transform.position, stats.HideTerrain, stats.HideSearchRadius, out hideTarget);
            if (!hideTargetSet)
            {
                hideTarget = homePosition;
            }

            if (stats != null && stats.HideWhileNotVisible && identity != null)
            {
                identity.SetVisible(false);
            }

            ChangeState(EnemyState.Hide);
        }

        private void UpdateHide()
        {
            if (hideTargetSet && Vector3.Distance(transform.position, hideTarget) > 0.35f)
            {
                float speed = stats != null ? stats.ChaseSpeed : 0f;
                movement.MoveTowards(hideTarget, speed);
                return;
            }

            movement.Stop();
            hideRemaining -= Time.deltaTime;

            if (hideRemaining <= 0f)
            {
                if (identity != null)
                {
                    identity.SetVisible(true);
                }

                if (stats != null && stats.HidesPeriodically)
                {
                    hideTimer = Random.Range(stats.HideIntervalMin, stats.HideIntervalMax);
                }

                ChangeState(canSeePlayer ? EnemyState.Chase : EnemyState.Patrol);
            }
        }

        private void EnterAlert()
        {
            lostTimer = 0f;
            alertTimer = alertTime;
            ChangeState(EnemyState.Alert);
        }

        private void ChangeState(EnemyState newState)
        {
            if (State == newState)
            {
                return;
            }

            OnExitState(State);
            State = newState;
            if (State != EnemyState.Chase)
            {
                blockedTimer = 0f;
            }

            if (health != null && stats != null && stats.ImmuneWhileIdle)
            {
                health.Invulnerable = State == EnemyState.Patrol || State == EnemyState.Return || State == EnemyState.Hide;
            }

            OnEnterState(State);
        }

        /// <summary>Punto de extensión al entrar en un estado.</summary>
        protected virtual void OnEnterState(EnemyState state) { }
        /// <summary>Punto de extensión al salir de un estado.</summary>
        protected virtual void OnExitState(EnemyState state) { }

        private void UpdateAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(stateParameter, (int)DetermineAnimationState());
        }

        /// <summary>Traduce el estado de la FSM al AnimationState del Animator.</summary>
        protected virtual AnimationState DetermineAnimationState()
        {
            switch (State)
            {
                case EnemyState.Dead:
                    return AnimationState.Death;

                case EnemyState.Attack:
                    return attack.AttackType == AttackType.Ranged
                        ? AnimationState.RangedAttack
                        : AnimationState.MeleeAttack;

                case EnemyState.Alert:
                    return AnimationState.Idle;

                case EnemyState.Hide:
                    return movement.IsMoving ? AnimationState.Move : AnimationState.Idle;

                case EnemyState.Patrol:
                    return patrolIsMoving ? AnimationState.Move : AnimationState.Idle;

                case EnemyState.Chase:
                case EnemyState.Return:
                    return AnimationState.Move;

                default:
                    return AnimationState.Idle;
            }
        }

        private void OnDied()
        {
            ChangeState(EnemyState.Dead);
            movement.Stop();
            SetCollidersEnabled(false);
            UpdateAnimation();

            if (identity != null)
            {
                identity.SetVisible(false);
            }

            Died?.Invoke(this);

            if (stats != null && stats.ExplodesOnDeath)
            {
                StartCoroutine(ExplosionRoutine());
                return;
            }

            SpawnFragmentDrop();

            if (deathLingerTime > 0f)
            {
                Destroy(gameObject, deathLingerTime);
            }
        }

        /// <summary>
        /// Cuenta regresiva y estallido del enemigo que explota al morir: sacudida de
        /// escala como aviso, daño en área al jugador, VFX y destrucción.
        /// </summary>
        private System.Collections.IEnumerator ExplosionRoutine()
        {
            float delay = stats != null ? stats.ExplosionDelay : 0.8f;
            float radius = stats != null ? stats.ExplosionRadius : 2f;
            int damage = stats != null ? stats.ExplosionDamage : 15;

            Vector3 baseScale = transform.localScale;

            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                float pulse = Mathf.Repeat(elapsed, 0.2f) / 0.2f;
                float shake = 1f + pulse * 0.15f;
                transform.localScale = new Vector3(baseScale.x * shake, baseScale.y * shake, baseScale.z);
                yield return null;
            }

            transform.localScale = baseScale;

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, 1 << playerLayer);
                for (int i = 0; i < hits.Length; i++)
                {
                    IDamageable damageable = hits[i].GetComponent<IDamageable>();
                    if (damageable == null)
                    {
                        damageable = hits[i].GetComponentInParent<IDamageable>();
                    }

                    damageable?.TakeDamage(damage);
                }
            }

            ExplosionVFX.Play(transform.position, radius);
            SpawnFragmentDrop();
            Destroy(gameObject);
        }

        private void SpawnFragmentDrop()
        {
            if (fragmentPickupPrefab != null)
            {
                GameObject drop = Instantiate(fragmentPickupPrefab, transform.position, Quaternion.identity);
                FragmentPickup pickup = drop.GetComponent<FragmentPickup>();
                if (pickup != null) pickup.Setup(fragmentsPerKill);
            }
            else if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.AddFragments(fragmentsPerKill);
            }
        }
        private void SetCollidersEnabled(bool value)
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = value;
            }
        }
    }
}
