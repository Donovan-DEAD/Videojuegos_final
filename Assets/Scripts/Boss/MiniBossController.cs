using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Miniboss de cuarto: persigue al jugador, ataca cuerpo a cuerpo y, opcionalmente,
    /// ejecuta una embestida con windup. Los valores se configuran por prefab:
    /// Lobo Alfa (bosque, rápido) y Capitán Garfio (mar, pesado).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MiniBossController : MonoBehaviour
    {
        [SerializeField] private BossAbilitySO rewardAbility;
        [SerializeField] private GameObject abilityPickupPrefab;
        [SerializeField] private BossIntroSO outro;

        [Header("Combat")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float attackRange = 1.6f;
        [SerializeField] private int attackDamage = 8;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float deathDelay = 1.5f;

        [Header("Charge")]
        [SerializeField] private bool useCharge = true;
        [SerializeField] private float chargeSpeed = 10f;
        [SerializeField] private float chargeDuration = 0.3f;
        [SerializeField] private float chargeCooldown = 3f;
        [SerializeField] private float chargeWindup = 0.35f;
        [SerializeField] private float chargeMinDistance = 2.5f;
        [SerializeField] private float chargeMaxDistance = 7f;
        [SerializeField] private int chargeDamage = 12;
        [SerializeField] private float chargeHitRadius = 1f;

        private enum MiniBossState { Chase, ChargeWindup, Charge, Recover }

        private Health health;
        private Rigidbody2D rb;
        private Transform player;
        private float nextAttack;
        private float nextCharge;
        private float stateTimer;
        private Vector2 chargeDirection;
        private bool chargeHit;
        private bool dead;
        private MiniBossState state = MiniBossState.Chase;

        private SpriteRenderer[] bodyRenderers;
        private Color[] baseColors;

        /// <summary>Inyecta la recompensa, el prefab de pickup y el outro (los llama el spawner).</summary>
        public void Setup(BossAbilitySO ability, GameObject pickupPrefab, BossIntroSO outroData = null)
        {
            rewardAbility = ability;
            if (pickupPrefab != null) abilityPickupPrefab = pickupPrefab;
            if (outroData != null) outro = outroData;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            rb = GetComponent<Rigidbody2D>();

            bodyRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            baseColors = new Color[bodyRenderers.Length];
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                baseColors[i] = bodyRenderers[i].color;
            }

            if (health != null) health.Died += OnDied;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
        }

        private void Update()
        {
            if (dead || health == null || health.CurrentHealth <= 0) return;

            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }

            switch (state)
            {
                case MiniBossState.Chase: UpdateChase(); break;
                case MiniBossState.ChargeWindup: UpdateChargeWindup(); break;
                case MiniBossState.Charge: UpdateCharge(); break;
                case MiniBossState.Recover: UpdateRecover(); break;
            }
        }

        private void UpdateChase()
        {
            if (player == null) return;

            Vector2 to = (Vector2)(player.position - transform.position);
            float dist = to.magnitude;

            if (useCharge && Time.time >= nextCharge && dist >= chargeMinDistance && dist <= chargeMaxDistance)
            {
                StartCharge(to);
                return;
            }

            if (dist <= EffectiveAttackRange())
            {
                StopMoving();
                if (Time.time >= nextAttack) TryAttack();
            }
            else if (dist > 0.001f)
            {
                rb.linearVelocity = to.normalized * moveSpeed;
            }
        }

        private void StartCharge(Vector2 to)
        {
            chargeDirection = to.normalized;
            chargeHit = false;
            stateTimer = chargeWindup;
            state = MiniBossState.ChargeWindup;
        }

        private void UpdateChargeWindup()
        {
            StopMoving();
            Flash(Mathf.PingPong(Time.time * 8f, 1f));
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                RestoreColors();
                nextCharge = Time.time + chargeCooldown;
                stateTimer = chargeDuration;
                state = MiniBossState.Charge;
            }
        }

        private void UpdateCharge()
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;

            if (!chargeHit)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chargeHitRadius);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (!hits[i].CompareTag("Player")) continue;
                    Health ph = hits[i].GetComponentInParent<Health>();
                    if (ph != null)
                    {
                        ph.TakeDamage(chargeDamage);
                        chargeHit = true;
                    }
                }
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                StopMoving();
                stateTimer = 0.3f;
                state = MiniBossState.Recover;
            }
        }

        private void UpdateRecover()
        {
            StopMoving();
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f) state = MiniBossState.Chase;
        }

        private float EffectiveAttackRange()
        {
            float range = attackRange;
            Collider2D self = GetComponent<Collider2D>();
            Collider2D other = player != null ? player.GetComponent<Collider2D>() : null;
            if (self != null && other != null)
            {
                float selfRadius = Mathf.Max(self.bounds.extents.x, self.bounds.extents.y);
                float otherRadius = Mathf.Max(other.bounds.extents.x, other.bounds.extents.y);
                range = Mathf.Max(range, selfRadius + otherRadius + 0.15f);
            }
            return range;
        }

        private void TryAttack()
        {
            nextAttack = Time.time + attackCooldown;
            Health ph = player.GetComponent<Health>();
            if (ph == null) ph = player.GetComponentInParent<Health>();
            if (ph != null) ph.TakeDamage(attackDamage);
        }

        private void Flash(float t)
        {
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                bodyRenderers[i].color = Color.Lerp(baseColors[i], Color.white, t);
            }
        }

        private void RestoreColors()
        {
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                bodyRenderers[i].color = baseColors[i];
            }
        }

        private void StopMoving()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        private void OnDied()
        {
            dead = true;
            RestoreColors();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            foreach (Collider2D c in GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            if (abilityPickupPrefab != null && rewardAbility != null)
            {
                GameObject pickup = Instantiate(abilityPickupPrefab, transform.position, Quaternion.identity);
                AbilityPickup ap = pickup.GetComponent<AbilityPickup>();
                if (ap != null) ap.Setup(rewardAbility);
            }

            if (outro != null && BossIntroController.Instance != null)
            {
                BossIntroController.Instance.Show(outro, null);
            }

            Destroy(gameObject, deathDelay);
        }
    }
}