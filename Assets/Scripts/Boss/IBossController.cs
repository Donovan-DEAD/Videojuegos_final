using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Contrato que expone un jefe al exterior: su fase actual y la configuración inicial.
    /// </summary>
    public interface IBossController
    {
        /// <summary>Fase actual del jefe (base cero).</summary>
        int CurrentPhase { get; }
        /// <summary>
        /// Inyecta las estadísticas del jefe y el objetivo (jugador) a perseguir.
        /// </summary>
        void ConfigureBoss(BossStatsSO stats, Transform target);
    }
}
