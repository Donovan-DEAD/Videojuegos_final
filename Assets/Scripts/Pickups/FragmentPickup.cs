using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Fragmento (moneda) que flota levemente y suelta pocas partículas moradas.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class FragmentPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 1;

        [Header("Animación")]
        [SerializeField] private float bobAmplitude = 0.12f;
        [SerializeField] private float bobSpeed = 2.2f;

        [Header("Partículas")]
        [SerializeField] private ParticleSystem particles;

        private Vector3 basePosition;

        private void Start()
        {
            basePosition = transform.position;
            if (particles == null) particles = GetComponentInChildren<ParticleSystem>(true);
        }

        private void Update()
        {
            float y = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = basePosition + new Vector3(0f, y, 0f);
        }

        public void Setup(int value)
        {
            amount = Mathf.Max(1, value);
        }

        private bool collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !other.CompareTag("Player")) return;
            collected = true;
            if (GameFlowManager.Instance != null) GameFlowManager.Instance.AddFragments(amount);
            Destroy(gameObject);
        }
    }
}
