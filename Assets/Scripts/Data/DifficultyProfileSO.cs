using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Ajuste de densidad de enemigos para un mundo a partir de un nivel mínimo.
    /// </summary>
    [System.Serializable]
    public struct DensityEntry
    {
        public int minLevel;
        public int maxTier;
        public float densityMultiplier;
        public int enemiesMin;
        public int enemiesMax;
    }

    /// <summary>
    /// Perfil de dificultad de un mundo: define la densidad de enemigos por nivel y los topes
    /// de enemigos según el tamaño de la sala.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyProfile", menuName = "Madness Dreams/Difficulty Profile")]
    public class DifficultyProfileSO : ScriptableObject
    {
        [SerializeField] private List<DensityEntry> entries = new List<DensityEntry>();

        [Header("Hard caps per room")]
        [SerializeField] private int maxEnemiesSmall = 2;
        [SerializeField] private int maxEnemiesMedium = 4;
        [SerializeField] private int maxEnemiesLarge = 6;
        [SerializeField] private int maxEnemiesExtraLarge = 9;

        [Header("Floor")]
        [SerializeField] private int minTierGuaranteed = 1;

        public int MinTierGuaranteed => Mathf.Max(1, minTierGuaranteed);

        /// <summary>
        /// Devuelve la entrada de densidad aplicable al nivel dado (la de mayor <c>minLevel</c> que
        /// no lo supere) o un valor por defecto si no hay coincidencias.
        /// </summary>
        public DensityEntry Resolve(int level)
        {
            DensityEntry best = default;
            bool found = false;

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].minLevel <= level && (!found || entries[i].minLevel >= best.minLevel))
                    {
                        best = entries[i];
                        found = true;
                    }
                }
            }

            if (!found)
            {
                best = new DensityEntry
                {
                    minLevel = 0,
                    maxTier = minTierGuaranteed,
                    densityMultiplier = 1f,
                    enemiesMin = 1,
                    enemiesMax = 2
                };
            }

            return best;
        }

        /// <summary>
        /// Devuelve el tope de enemigos correspondiente al tamaño de la sala en celdas.
        /// </summary>
        public int MaxEnemiesForRoom(Vector2Int sizeInCells)
        {
            int cells = RoomSizes.CellCount(sizeInCells);

            if (cells <= 1)
            {
                return maxEnemiesSmall;
            }

            if (cells <= 2)
            {
                return maxEnemiesMedium;
            }

            if (cells <= 4)
            {
                return maxEnemiesLarge;
            }

            return maxEnemiesExtraLarge;
        }
    }
}
