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

        /// <summary>Estado actual de la FSM.</summary>
        public EnemyState State { get; private set; } = EnemyState.Patrol;
        /// <summary>Se dispara al morir el enemigo, antes de destruirse.</summary>
        public event System.Action<EnemyAI> Died;

        protected EnemyMovement movement;
        protected EnemyAttack attack;
        protected EnemyFOV fov;
        protected Health health;
        protected Transform player;
        protected IDamageable playerDamageable;

        private Vector3 homePosition;
        private float fovTimer;
        private bool canSeePlayer;
        private float alertTimer;
        private float lostTimer;
        private Vector2 patrolDirection;
        private float patrolTimer;
        private bool patrolIsMoving;

        /// <summary>Cachea los componentes requeridos, guarda la posición inicial y aplica las estadísticas.</summary>
        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyAttack>();
            fov = GetComponent<EnemyFOV>();
            health = GetComponent<Health>();

            homePosition = transform.position;
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

            switch (State)
            {
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Alert: UpdateAlert(); break;
                case EnemyState.Chase: UpdateChase(); break;
                case EnemyState.Attack: UpdateAttack(); break;
                case EnemyState.Return: UpdateReturn(); break;
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
            health.SetMaxHealth(stats.MaxHealth);
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
            if (canSeePlayer)
            {
                EnterAlert();
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

            float speed = stats != null ? stats.ChaseSpeed : 0f;
            movement.MoveTowards(player.position, speed);
        }

        private void UpdateAttack()
        {
            if (player == null)
            {
                ChangeState(EnemyState.Return);
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

            float speed = stats != null ? stats.MoveSpeed : 0f;
            movement.MoveTowards(homePosition, speed);

            if (Vector3.Distance(transform.position, homePosition) <= 0.2f)
            {
                ChangeState(EnemyState.Patrol);
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
            Died?.Invoke(this);

            if (deathLingerTime > 0f)
            {
                Destroy(gameObject, deathLingerTime);
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
