using UnityEngine;

namespace MadnessDreams
{
    /// <summary>Muestra u oculta la guía visual de teclas del HUD.</summary>
    public class KeyGuideToggle : MonoBehaviour
    {
        [SerializeField] private GameObject guide;

        public void Toggle()
        {
            if (guide != null) guide.SetActive(!guide.activeSelf);
        }
    }
}
