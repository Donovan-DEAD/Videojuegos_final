using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Menú principal: permite elegir protagonista y arranca la partida, abre ajustes o cierra el juego.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button characterToggle;
        [SerializeField] private Text characterLabel;

        private CharacterId selectedCharacter = CharacterId.Hansel;

        /// <summary>
        /// Enlaza los botones de jugar, ajustes, salir y alternar personaje.
        /// </summary>
        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlay);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettings);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuit);
            }

            if (characterToggle != null)
            {
                characterToggle.onClick.AddListener(ToggleCharacter);
            }
        }

        /// <summary>
        /// Recupera el personaje seleccionado desde GameFlowManager y actualiza la etiqueta.
        /// </summary>
        private void Start()
        {
            if (GameFlowManager.Instance != null)
            {
                selectedCharacter = GameFlowManager.Instance.SelectedCharacter;
            }

            UpdateCharacterLabel();
        }

        private void ToggleCharacter()
        {
            selectedCharacter = selectedCharacter == CharacterId.Hansel ? CharacterId.Gretel : CharacterId.Hansel;
            UpdateCharacterLabel();
        }

        private void UpdateCharacterLabel()
        {
            if (characterLabel != null)
            {
                characterLabel.text = "Protagonista: " + selectedCharacter;
            }
        }

        private void OnPlay()
        {
            GameFlowManager.Instance.StartRun(selectedCharacter);
        }

        private void OnSettings()
        {
            GameFlowManager.Instance.OpenSettings(GameFlowManager.MainMenuScene);
        }

        private void OnQuit()
        {
            GameFlowManager.Instance.QuitGame();
        }
    }
}
