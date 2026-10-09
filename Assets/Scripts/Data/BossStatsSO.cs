using UnityEngine;

namespace MadnessDreams
{
    [CreateAssetMenu(fileName = "BossStats", menuName = "Madness Dreams/Boss Stats")]
    public class BossStatsSO : EnemyStatsSO
    {
        [Header("Phases")]
        [Tooltip("Health ratios (0..1, ascending) that trigger a phase change. [0.66, 0.33] = phase 1 until 66%, then phase 2 until 33%, then phase 3.")]
        [SerializeField] private float[] phaseHealthThresholds = new float[] { 0.66f, 0.33f };

        [Tooltip("Attack prefabs/patterns per phase. Index 0 = phase 1, and so on.")]
        [SerializeField] private GameObject[] phaseAttacks;

        public float[] PhaseHealthThresholds => phaseHealthThresholds;
        public GameObject[] PhaseAttacks => phaseAttacks;
    }
}
