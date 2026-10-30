using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Arena de jefe: bloquea las salidas mientras hay combate, instancia al jefe en el punto
    /// de aparición, configura su IA y notifica su derrota.
    /// </summary>
    public class BossArenaController : MonoBehaviour
    {
        [SerializeField] private Collider2D[] exitBlockers;
        [SerializeField] private BossSpawnPoint bossSpawnPoint;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private BossStatsSO bossStats;
        [SerializeField] private BossIntroSO intro;

        public bool BossActive { get; private set; }
        public bool BossDied { get; private set; }
        public GameObject BossInstance => bossInstance;
        /// <summary>
        /// Se dispara cuando el jefe invocado en la arena es derrotado.
        /// </summary>
        public event System.Action BossDefeated;
        /// <summary>
        /// Se dispara al invocar al jefe, entregando su <see cref="Health"/> y el nombre del prefab.
        /// </summary>
        public event System.Action<Health, string> BossSpawned;

        private GameObject bossInstance;

        /// <summary>
        /// Localiza el <see cref="BossSpawnPoint"/> entre los hijos si no se asignó en el inspector.
        /// </summary>
        private void Awake()
        {
            if (bossSpawnPoint == null)
            {
                bossSpawnPoint = GetComponentInChildren<BossSpawnPoint>();
            }
        }

        /// <summary>
        /// Define el prefab del jefe y sus estadísticas antes de iniciar el combate.
        /// </summary>
        public void Setup(GameObject prefab, BossStatsSO stats, BossIntroSO introData = null)
        {
            bossPrefab = prefab;
            bossStats = stats;
            if (introData != null) intro = introData;
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

        /// <summary>
        /// Inicia el combate: bloquea las salidas, instancia al jefe, lo configura vía
        /// <see cref="IBossController"/> y suscribe su derrota. No hace nada si ya está activo.
        /// </summary>
        public void StartBossFight()
        {
            if (BossActive || BossDied)
            {
                return;
            }

            BossActive = true;
            SetExitsBlocked(true);

            if (BossIntroController.Instance != null && intro != null)
            {
                BossIntroController.Instance.Show(intro, SpawnBoss);
            }
            else
            {
                SpawnBoss();
            }
        }

        private void SpawnBoss()
        {
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
            BossDied = true;

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
