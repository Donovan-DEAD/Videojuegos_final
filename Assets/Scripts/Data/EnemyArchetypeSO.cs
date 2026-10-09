using UnityEngine;

namespace MadnessDreams
{
    [CreateAssetMenu(fileName = "EnemyArchetype", menuName = "Madness Dreams/Enemy Archetype")]
    public class EnemyArchetypeSO : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private EnemyStatsSO stats;

        public GameObject Prefab => prefab;
        public EnemyStatsSO Stats => stats;
    }
}
