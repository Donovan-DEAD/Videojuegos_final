using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Fragmento (moneda) que el jugador recoge al tocarlo.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class FragmentPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 1;

        public void Setup(int value)
        {
            amount = Mathf.Max(1, value);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (GameFlowManager.Instance != null) GameFlowManager.Instance.AddFragments(amount);
            Destroy(gameObject);
        }
    }
}
