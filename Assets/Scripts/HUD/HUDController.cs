using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Controlador del HUD: enlaza al jugador, el MapGenerator y el BossArenaController, y delega la actualización en cada widget.
    /// </summary>
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

        /// <summary>
        /// Busca al jugador, el mapa y la arena, enlaza los widgets y se suscribe a los eventos del BossArenaController.
        /// </summary>
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
            

            if (mapOverlay != null) mapOverlay.Bind(mapGenerator, playerTransform);
            if (minimapButton != null && mapOverlay != null) minimapButton.onClick.AddListener(mapOverlay.Toggle);

            arena = FindAnyObjectByType<BossArenaController>();
            if (arena != null)
            {
                arena.BossSpawned += OnBossSpawned;
                arena.BossDefeated += OnBossDefeated;
            }
        }

        /// <summary>
        /// Se desuscribe de los eventos del BossArenaController al destruirse.
        /// </summary>
        private void OnDestroy()
        {
            if (arena != null)
            {
                arena.BossSpawned -= OnBossSpawned;
                arena.BossDefeated -= OnBossDefeated;
            }
        }

        /// <summary>
        /// Refresca cada fotograma el widget de enfriamiento de ataque.
        /// </summary>
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
