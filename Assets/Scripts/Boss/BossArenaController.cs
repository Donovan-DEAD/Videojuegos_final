using UnityEngine;

namespace MadnessDreams
{
    public class BossArenaController : MonoBehaviour
    {
        [SerializeField] private Collider2D[] exitBlockers;
        [SerializeField] private BossSpawnPoint bossSpawnPoint;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private BossStatsSO bossStats;

        public bool BossActive { get; private set; }
        public GameObject BossInstance => bossInstance;
        public event System.Action BossDefeated;
        public event System.Action<Health, string> BossSpawned;

        private GameObject bossInstance;

        private void Awake()
        {
            if (bossSpawnPoint == null)
            {
                bossSpawnPoint = GetComponentInChildren<BossSpawnPoint>();
            }
        }

        public void Setup(GameObject prefab, BossStatsSO stats)
        {
            bossPrefab = prefab;
            bossStats = stats;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (BossActive)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            StartBossFight();
        }

        public void StartBossFight()
        {
            if (BossActive)
            {
                return;
            }

            BossActive = true;
            SetExitsBlocked(true);

            if (bossSpawnPoint == null || bossPrefab == null)
            {
                return;
            }

            bossInstance = Instantiate(bossPrefab, bossSpawnPoint.Position, Quaternion.identity);

            IBossController boss = bossInstance.GetComponent<IBossController>();
            if (boss != null)
            {
                Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
                boss.ConfigureBoss(bossStats, player);
            }

            Health bossHealth = bossInstance.GetComponent<Health>();
            if (bossHealth != null)
            {
                bossHealth.Died += OnBossDefeated;
                BossSpawned?.Invoke(bossHealth, bossPrefab != null ? bossPrefab.name : bossInstance.name);
            }
        }

        private void OnBossDefeated()
        {
            SetExitsBlocked(false);
            BossActive = false;

            if (bossInstance != null)
            {
                Health bossHealth = bossInstance.GetComponent<Health>();
                if (bossHealth != null)
                {
                    bossHealth.Died -= OnBossDefeated;
                }
            }

            BossDefeated?.Invoke();
        }

        private void SetExitsBlocked(bool blocked)
        {
            if (exitBlockers == null)
            {
                return;
            }

            foreach (Collider2D blocker in exitBlockers)
            {
                if (blocker != null)
                {
                    blocker.enabled = blocked;
                }
            }
        }
    }
}
