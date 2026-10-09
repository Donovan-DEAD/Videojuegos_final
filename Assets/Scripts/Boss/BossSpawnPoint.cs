using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Marca en la escena la posición donde aparece el jefe y la representa con gizmos.
    /// </summary>
    public class BossSpawnPoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
