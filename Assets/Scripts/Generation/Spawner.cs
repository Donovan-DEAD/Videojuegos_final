using UnityEngine;

namespace MadnessDreams
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private int maxTries = 8;
        [SerializeField] private float clearanceRadius = 0.5f;
        [SerializeField] private LayerMask obstacleMask = ~0;

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
