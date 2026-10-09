using UnityEngine;

namespace MadnessDreams
{
    public interface IBossController
    {
        int CurrentPhase { get; }
        void ConfigureBoss(BossStatsSO stats, Transform target);
    }
}
