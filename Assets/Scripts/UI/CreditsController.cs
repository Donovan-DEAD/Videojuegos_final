using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Pantalla temporal de créditos: vuelve al menú principal.</summary>
    public class CreditsController : MonoBehaviour
    {
        [SerializeField] private Button menuButton;

        private void Start()
        {
            if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
        }

        private void GoToMenu()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToMainMenu();
            }
        }
    }
}
