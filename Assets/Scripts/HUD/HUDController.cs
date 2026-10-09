using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private HealthBarWidget healthBar;
        [SerializeField] private AttackCooldownWidget attacks;
        [SerializeField] private MinimapWidget minimap;
        [SerializeField] private TaleFragmentWidget fragments;
        [SerializeField] private BossHealthBarWidget bossBar;
        [SerializeField] private Button minimapButton;
        [SerializeField] private MapOverlayWidget mapOverlay;

        private BossArenaController arena;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Health health = player != null ? player.GetComponent<Health>() : null;
            PlayerCombat combat = player != null ? player.GetComponent<PlayerCombat>() : null;
            Transform playerTransform = player != null ? player.transform : null;
            MapGenerator mapGenerator = FindAnyObjectByType<MapGenerator>();

            if (healthBar != null) healthBar.Bind(health);
            if (attacks != null) attacks.Bind(combat);
            if (minimap != null) minimap.Bind(mapGenerator, playerTransform);
            if (fragments != null) fragments.SetCount(0);

            if (mapOverlay != null) mapOverlay.Bind(mapGenerator, playerTransform);
            if (minimapButton != null && mapOverlay != null) minimapButton.onClick.AddListener(mapOverlay.Toggle);

            arena = FindAnyObjectByType<BossArenaController>();
            if (arena != null)
            {
                arena.BossSpawned += OnBossSpawned;
                arena.BossDefeated += OnBossDefeated;
            }
        }

        private void OnDestroy()
        {
            if (arena != null)
            {
                arena.BossSpawned -= OnBossSpawned;
                arena.BossDefeated -= OnBossDefeated;
            }
        }

        private void LateUpdate()
        {
            if (attacks != null)
            {
                attacks.Refresh();
            }
        }

        private void OnBossSpawned(Health health, string bossName)
        {
            if (bossBar != null)
            {
                bossBar.Show(bossName, health);
            }
        }

        private void OnBossDefeated()
        {
            if (bossBar != null)
            {
                bossBar.Hide();
            }
        }
    }
}
