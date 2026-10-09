using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Revela el portal cuando muere el jefe: escucha BossManager.BossDefeated o, si no hay BossManager, engancha el Health del jefe de la escena (IBossController).
    /// </summary>
    public class WorldPortalController : MonoBehaviour
    {
        [SerializeField] private BossManager bossManager;
        [SerializeField] private GameObject portal;
        [SerializeField] private Transform spawnPoint;

        private Health directBossHealth;

        /// <summary>
        /// Oculta el portal, localiza el BossManager y se suscribe a su derrota (o engancha el jefe de la escena si no hay).
        /// </summary>
        private void Start()
        {
            if (portal != null)
            {
                portal.SetActive(false);
            }
            else
            {
                Debug.LogWarning("WorldPortalController: no hay portal asignado.", this);
            }

            if (bossManager == null)
            {
                bossManager = FindAnyObjectByType<BossManager>();
            }

            if (bossManager != null)
            {
                bossManager.BossDefeated += OnBossDefeated;
                return;
            }

            HookSceneBoss();
        }

        private void HookSceneBoss()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (!(behaviours[i] is IBossController))
                {
                    continue;
                }

                directBossHealth = behaviours[i].GetComponent<Health>();
                break;
            }

            if (directBossHealth != null)
            {
                directBossHealth.Died += OnBossDefeated;
            }
        }

        /// <summary>
        /// Deja de escuchar la derrota del jefe al destruirse.
        /// </summary>
        private void OnDestroy()
        {
            if (bossManager != null)
            {
                bossManager.BossDefeated -= OnBossDefeated;
            }

            if (directBossHealth != null)
            {
                directBossHealth.Died -= OnBossDefeated;
            }
        }

        private void OnBossDefeated()
        {
            if (portal == null)
            {
                return;
            }

            if (spawnPoint != null)
            {
                portal.transform.position = spawnPoint.position;
            }

            portal.SetActive(true);
        }
    }
}
