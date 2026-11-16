using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Recogible de habilidad: enseña la BossAbilitySO al jugador por trigger o por cercanía.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class AbilityPickup : MonoBehaviour
    {
        [SerializeField] private BossAbilitySO ability;
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float pickupRadius = 1.5f;

        private Vector3 startPosition;
        private Transform player;

        private void Start()
        {
            startPosition = transform.position;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        private void Update()
        {
            if (bobAmplitude > 0f)
            {
                transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            }

            if (player != null && Vector2.Distance(transform.position, player.position) <= pickupRadius)
            {
                TryCollect(player.gameObject);
            }
        }

        public void Setup(BossAbilitySO newAbility)
        {
            ability = newAbility;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            TryCollect(other.gameObject);
        }

        private void TryCollect(GameObject playerObject)
        {
            if (ability == null) return;

            PlayerAbilities abilities = playerObject.GetComponent<PlayerAbilities>();
            if (abilities == null) abilities = playerObject.GetComponentInParent<PlayerAbilities>();
            if (abilities == null) return;

            if (abilities.Learn(ability))
            {
                if (ability.AcquiredIntro != null && BossIntroController.Instance != null)
                {
                    BossIntroController.Instance.Show(ability.AcquiredIntro, null);
                }

                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
