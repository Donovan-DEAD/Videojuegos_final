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
        [SerializeField] private GameObject abilityPickupPrefab;

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

            arena.Setup(config.BossPrefab, config.Boss, config.BossIntro);
            arena.BossDefeated += OnBossDefeated;
        }

        /// <summary>Suelta la habilidad de recompensa del mundo en la posición del jefe derrotado.</summary>
        private void SpawnReward()
        {
            if (abilityPickupPrefab == null)
            {
                Debug.LogWarning("BossManager.SpawnReward: falta abilityPickupPrefab.", this);
                return;
            }
            
            if (config == null || config.RewardAbility == null)
            {
                Debug.LogWarning("BossManager.SpawnReward: falta config o RewardAbility.", this);
                return;
            }
        
            Vector3 position = arena != null && arena.BossInstance != null ? arena.BossInstance.transform.position : transform.position;
            GameObject pickup = Instantiate(abilityPickupPrefab, position, Quaternion.identity);
            AbilityPickup abilityPickup = pickup.GetComponent<AbilityPickup>();
            if (abilityPickup != null)
            {
                abilityPickup.Setup(config.RewardAbility);
            }
        }
        private void OnBossDefeated()
        {
            Debug.Log("Boss defeated.");
            SpawnReward();

            if (config != null && config.BossOutro != null && BossIntroController.Instance != null)
            {
                BossIntroController.Instance.Show(config.BossOutro, () => BossDefeated?.Invoke());
            }
            else
            {
                BossDefeated?.Invoke();
            }
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
