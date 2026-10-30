using PrimeTween;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Configuración global de PrimeTween al arrancar.</summary>
    public static class PrimeTweenConfigSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        }
    }
}
