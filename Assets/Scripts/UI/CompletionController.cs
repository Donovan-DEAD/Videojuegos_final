using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>Pantalla de finalización de la run: permite ir a créditos o al menú principal.</summary>
    public class CompletionController : MonoBehaviour
    {
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button menuButton;

        private void Start()
        {
            if (creditsButton != null) creditsButton.onClick.AddListener(GoToCredits);
            if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
        }

        private void GoToCredits()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.LoadSceneWithLoading(GameFlowManager.CreditsScene);
            }
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
