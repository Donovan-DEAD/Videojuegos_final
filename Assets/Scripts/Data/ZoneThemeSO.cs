using System.Collections.Generic;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Enemigo con su peso relativo dentro de un banco.
    /// </summary>
    [System.Serializable]
    public struct WeightedEnemy
    {
        public EnemyArchetypeSO archetype;
        public int weight;
    }

    /// <summary>
    /// Prop (o barrera) con su peso relativo dentro de un banco.
    /// </summary>
    [System.Serializable]
    public struct WeightedProp
    {
        public PropDefinitionSO prop;
        public int weight;
    }

    /// <summary>
    /// Tema de una zona: etiqueta, bancos ponderados de enemigos, props y barreras, y su
    /// densidad base de aparición.
    /// </summary>
    [CreateAssetMenu(fileName = "ZoneTheme", menuName = "Madness Dreams/Zone Theme")]
    public class ZoneThemeSO : ScriptableObject
    {
        [Header("Tag")]
        [SerializeField] private RoomTag tag = RoomTag.None;

        [Header("Banks")]
        [SerializeField] private List<WeightedEnemy> enemyBank = new List<WeightedEnemy>();
        [SerializeField] private List<WeightedProp> propBank = new List<WeightedProp>();
        [SerializeField] private List<WeightedProp> barrierBank = new List<WeightedProp>();

        [Header("Base density (the difficulty profile modulates it)")]
        [SerializeField] private int enemiesMin = 1;
        [SerializeField] private int enemiesMax = 3;
        [SerializeField] private int propsMin = 2;
        [SerializeField] private int propsMax = 6;
        [SerializeField] private int barriersMin = 1;
        [SerializeField] private int barriersMax = 3;

        public RoomTag Tag => tag;
        public List<WeightedEnemy> EnemyBank => enemyBank;
        public List<WeightedProp> PropBank => propBank;
        public List<WeightedProp> BarrierBank => barrierBank;
        public int EnemiesMin => Mathf.Max(0, enemiesMin);
        public int EnemiesMax => Mathf.Max(enemiesMin, enemiesMax);
        public int PropsMin => Mathf.Max(0, propsMin);
        public int PropsMax => Mathf.Max(propsMin, propsMax);
        public int BarriersMin => Mathf.Max(0, barriersMin);
        public int BarriersMax => Mathf.Max(barriersMin, barriersMax);
    }
}
