using UnityEngine;

namespace MadnessDreams
{
    [RequireComponent(typeof(Collider2D))]
    public class AbilityPickup : MonoBehaviour
    {
        [SerializeField] private BossAbilitySO ability;
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (bobAmplitude > 0f)
            {
                transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            PlayerAbilities abilities = other.GetComponent<PlayerAbilities>();
            if (abilities == null)
            {
                abilities = other.GetComponentInParent<PlayerAbilities>();
            }

            if (abilities == null)
            {
                return;
            }

            if (abilities.Learn(ability) && destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
