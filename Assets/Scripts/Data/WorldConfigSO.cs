using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Asocia una etiqueta de sala con el tema de zona que le corresponde.
    /// </summary>
    [System.Serializable]
    public struct ThemeEntry
    {
        public RoomTag tag;
        public ZoneThemeSO theme;
        public int weight;
    }

    /// <summary>
    /// Configuración de un mundo: semilla, dimensiones del mapa, temas por etiqueta,
    /// dificultad, enemigos y jefe.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Madness Dreams/World Config")]
    public class WorldConfigSO : ScriptableObject
    {
        [Header("Seed")]
        [SerializeField] private int seed = 0;

        [Header("Map")]
        [SerializeField] private BlockBankSO bank;
        [SerializeField] private int width = 14;
        [SerializeField] private int height = 14;
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private int bossRegionSize = 3;

        [Header("Layout")]
        [SerializeField] private int[] sizeWeights = { 4, 3, 1, 1 };
        [SerializeField] private int openingWidth = 4;
        [SerializeField, Range(0.1f, 1f)] private float fillRatio = 1f;

        [Header("Themes")]
        [SerializeField] private List<ThemeEntry> themes = new List<ThemeEntry>();

        [Header("Tileset")]
        [SerializeField] private TilesetCatalogSO tileset;

        [Header("Difficulty")]
        [SerializeField] private DifficultyProfileSO difficulty;

        [Header("Enemies (legacy)")]
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
        public int[] SizeWeights => sizeWeights;
        public int OpeningWidth => openingWidth;
        public float FillRatio => fillRatio;
        public List<ThemeEntry> Themes => themes;
        public TilesetCatalogSO Tileset => tileset;
        public DifficultyProfileSO Difficulty => difficulty;
        public List<EnemyArchetypeSO> EnemyPool => enemyPool;
        public int EnemiesPerChunk => enemiesPerChunk;
        public BossStatsSO Boss => boss;
        public GameObject BossPrefab => bossPrefab;

        /// <summary>
        /// Devuelve el peso de aparición de la clase de tamaño indicada, con 1 como valor por
        /// defecto si no está definido.
        /// </summary>
        public int WeightFor(RoomSizeClass sizeClass)
        {
            if (sizeWeights == null)
            {
                return 1;
            }

            int index = (int)sizeClass;
            if (index < 0 || index >= sizeWeights.Length)
            {
                return 1;
            }

            return Mathf.Max(0, sizeWeights[index]);
        }

        /// <summary>
        /// Devuelve el tema de zona asociado a la etiqueta de sala, o null si no hay ninguno.
        /// </summary>
        public ZoneThemeSO ThemeFor(RoomTag tag)
        {
            if (themes == null)
            {
                return null;
            }

            for (int i = 0; i < themes.Count; i++)
            {
                if (themes[i].tag == tag)
                {
                    return themes[i].theme;
                }
            }

            return null;
        }

        /// <summary>Peso de un tema del mundo (mínimo 1, para que un peso en 0 no lo desactive por accidente).</summary>
        public int TagWeight(int index)
        {
            if (themes == null || index < 0 || index >= themes.Count)
            {
                return 0;
            }

            return Mathf.Max(1, themes[index].weight);
        }
    }
}
