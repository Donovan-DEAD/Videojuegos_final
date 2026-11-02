using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Ráfaga de invocación: emite partículas y se autodestruye.</summary>
    public class SummonEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private int burst = 40;
        [SerializeField] private float lifetime = 1.2f;

        private void Start()
        {
            if (particles == null) particles = GetComponentInChildren<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
                particles.Emit(burst);
            }
            Destroy(gameObject, lifetime);
        }
    }
}
