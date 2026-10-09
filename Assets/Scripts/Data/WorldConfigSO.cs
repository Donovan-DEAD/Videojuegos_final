using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Madness Dreams/World Config")]
    public class WorldConfigSO : ScriptableObject
    {
        [Header("Seed")]
        [SerializeField] private int seed = 0;

        [Header("Map")]
        [SerializeField] private BlockBankSO bank;
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private int bossRegionSize = 3;

        [Header("Enemies")]
        [SerializeField] private List<EnemyArchetypeSO> enemyPool = new List<EnemyArchetypeSO>();
        [SerializeField] private int enemiesPerChunk = 3;

        [Header("Boss")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private BossStatsSO boss;

        public int Seed => seed;
        public BlockBankSO Bank => bank;
        public int Width => width;
        public int Height => height;
        public int ChunkSize => chunkSize;
        public int BossRegionSize => bossRegionSize;
        public List<EnemyArchetypeSO> EnemyPool => enemyPool;
        public int EnemiesPerChunk => enemiesPerChunk;
        public BossStatsSO Boss => boss;
        public GameObject BossPrefab => bossPrefab;
    }
}
