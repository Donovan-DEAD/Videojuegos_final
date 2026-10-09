using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Portal que avanza al siguiente mundo: al entrar el jugador solicita a RunManager.RequestAdvance una única vez.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Portal : MonoBehaviour
    {
        private bool used;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            used = true;

            RunManager runManager = FindAnyObjectByType<RunManager>();
            if (runManager != null)
            {
                runManager.RequestAdvance();
            }
        }
    }
}
