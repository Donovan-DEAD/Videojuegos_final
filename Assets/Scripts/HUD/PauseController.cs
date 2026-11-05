using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Controla la pausa: se alterna con Escape y gestiona los paneles, congelando el juego (Time.timeScale 0/1).
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private SettingsController settingsController;

        public bool IsPaused { get; private set; }

        /// <summary>
        /// Enlaza los botones, se suscribe al cierre de ajustes y oculta los paneles al arrancar.
        /// </summary>
        private void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
            if (settingsController != null) settingsController.Closed += CloseSettings;

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void Update()
        {
            if (BossIntroController.IsShowing || LoadingScreen.IsShowing) return;

            if (WasPausePressed())
            {
                if (IsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        private static bool WasPausePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                return keyboard.escapeKey.wasPressedThisFrame;
            }

            return Input.GetKeyDown(KeyCode.Escape);
        }

        /// <summary>
        /// Pausa el juego (Time.timeScale 0) y muestra el panel de pausa.
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null) pausePanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        /// <summary>
        /// Reanuda el juego (Time.timeScale 1) y oculta los paneles.
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OpenSettings()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private void GoToMenu()
        {
            Time.timeScale = 1f;

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToMainMenu();
            }
        }
    }
}
