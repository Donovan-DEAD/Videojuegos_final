using UnityEngine;

namespace MadnessDreams
{
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

        private Health health;
        private Rigidbody2D rb;
        private Transform player;
        private float nextAttack;
        private bool dead;

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
            if (dead || player == null || health == null || health.CurrentHealth <= 0) return;

            Vector2 to = (Vector2)(player.position - transform.position);
            float dist = to.magnitude;

            if (dist <= attackRange)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                if (Time.time >= nextAttack) TryAttack();
            }
            else if (rb != null && dist > 0.001f)
            {
                rb.linearVelocity = to.normalized * moveSpeed;
            }
        }

        private void TryAttack()
        {
            nextAttack = Time.time + attackCooldown;
            Health ph = player.GetComponent<Health>();
            if (ph != null) ph.TakeDamage(attackDamage);
        }

        private void OnDied()
        {
            dead = true;
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
