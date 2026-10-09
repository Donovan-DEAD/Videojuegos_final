using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Coordina el combate de jefe de un mundo: entrega la configuración a la arena y
    /// reemite el evento de jefe derrotado.
    /// </summary>
    public class BossManager : MonoBehaviour
    {
        [SerializeField] private WorldConfigSO config;
        [SerializeField] private BossArenaController arena;

        /// <summary>
        /// Se dispara cuando la arena notifica que el jefe del mundo fue derrotado.
        /// </summary>
        public event System.Action BossDefeated;

        /// <summary>
        /// Asigna el <see cref="WorldConfigSO"/> que aporta el prefab y las estadísticas del jefe.
        /// </summary>
        public void Configure(WorldConfigSO worldConfig)
        {
            config = worldConfig;
        }

        /// <summary>
        /// Configura la arena con los datos del mundo y se suscribe a su evento de derrota.
        /// </summary>
        public void SetupBossFight()
        {
            if (arena == null || config == null)
            {
                return;
            }

            arena.Setup(config.BossPrefab, config.Boss);
            arena.BossDefeated += OnBossDefeated;
        }

        private void OnBossDefeated()
        {
            Debug.Log("Boss defeated.");
            BossDefeated?.Invoke();
        }

        /// <summary>
        /// Se desuscribe del evento de derrota de la arena para evitar referencias colgantes.
        /// </summary>
        private void OnDestroy()
        {
            if (arena != null)
            {
                arena.BossDefeated -= OnBossDefeated;
            }
        }
    }
}
