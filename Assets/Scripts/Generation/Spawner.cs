using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Instancia prefabs en posiciones válidas, aplicando un leve jitter y comprobando que no haya
    /// obstáculos en un radio de holgura.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private int maxTries = 8;
        [SerializeField] private float clearanceRadius = 0.5f;
        [SerializeField] private LayerMask obstacleMask = ~0;

        /// <summary>Intenta instanciar el prefab cerca de la posición dada evitando obstáculos; devuelve si lo logró y la instancia.</summary>
        public bool TrySpawn(GameObject prefab, Vector3 position, System.Random rng, out GameObject instance)
        {
            instance = null;
            if (prefab == null)
            {
                return false;
            }

            float jitter = clearanceRadius * 0.5f;

            for (int i = 0; i < maxTries; i++)
            {
                float dx = (float)(rng.NextDouble() * 2.0 - 1.0) * jitter;
                float dy = (float)(rng.NextDouble() * 2.0 - 1.0) * jitter;
                Vector3 candidate = position + new Vector3(dx, dy, 0f);

                if (Physics2D.OverlapCircle(candidate, clearanceRadius, obstacleMask) == null)
                {
                    instance = Instantiate(prefab, candidate, Quaternion.identity);
                    return true;
                }
            }

            return false;
        }
    }
}
