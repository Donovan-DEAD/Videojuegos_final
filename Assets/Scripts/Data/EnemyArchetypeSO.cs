using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Entrada de un enemigo en un banco: asocia su prefab, sus estadísticas, su nivel y su
    /// peso de aparición.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyArchetype", menuName = "Madness Dreams/Enemy Archetype")]
    public class EnemyArchetypeSO : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private EnemyStatsSO stats;
        [SerializeField] private int tier = 1;
        [SerializeField] private int weight = 1;

        public GameObject Prefab => prefab;
        public EnemyStatsSO Stats => stats;
        public int Tier => Mathf.Max(1, tier);
        public int Weight => Mathf.Max(0, weight);
    }
}
