using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Recogible que, al tocar al jugador, le enseña una BossAbilitySO y opcionalmente se destruye;
    /// además aplica un ligero vaivén visual.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AbilityPickup : MonoBehaviour
    {
        [SerializeField] private BossAbilitySO ability;
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;

        /// <summary>Guarda la posición inicial usada para el vaivén.</summary>
        private void Start()
        {
            startPosition = transform.position;
        }

        /// <summary>Aplica el vaivén vertical mientras bobAmplitude sea mayor que cero.</summary>
        private void Update()
        {
            if (bobAmplitude > 0f)
            {
                transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            }
        }

        /// <summary>Si lo toca el jugador, intenta enseñar la habilidad y se destruye si tuvo éxito.</summary>
        /// <summary>Asigna la habilidad que este recogible enseñará al jugador.</summary>
        public void Setup(BossAbilitySO newAbility)
        {
            ability = newAbility;
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
