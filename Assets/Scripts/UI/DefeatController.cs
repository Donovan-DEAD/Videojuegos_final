using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Pantalla de derrota cuyos botones reintentan la partida, vuelven al menú o cierran el juego mediante GameFlowManager.
    /// </summary>
    public class DefeatController : MonoBehaviour
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;

        /// <summary>
        /// Enlaza los botones de reintentar, menú y salir.
        /// </summary>
        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetry);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenu);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuit);
            }
        }

        private void OnRetry()
        {
            GameFlowManager.Instance.Retry();
        }

        private void OnMenu()
        {
            GameFlowManager.Instance.GoToMainMenu();
        }

        private void OnQuit()
        {
            GameFlowManager.Instance.QuitGame();
        }
    }
}
