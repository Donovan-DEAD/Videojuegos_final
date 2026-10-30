using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Muestra u oculta las etiquetas de teclas del HUD.</summary>
    public class KeyLabelsToggle : MonoBehaviour
    {
        [SerializeField] private GameObject[] targets;
        public static bool Visible { get; private set; } = true;

        public void Toggle()
        {
            Visible = !Visible;
            if (targets == null) return;
            foreach (var t in targets) if (t != null) t.SetActive(Visible);
        }
    }
}
