using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Pantalla de ajustes: enlaza los sliders y el toggle con SettingsData y guarda al pulsar Volver.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        /// <summary>
        /// Se dispara al pulsar Volver tras guardar los ajustes (lo escucha PauseController).
        /// </summary>
        public event System.Action Closed;

        private SettingsData data;

        /// <summary>
        /// Obtiene los datos desde GameFlowManager y enlaza los controles de la UI.
        /// </summary>
        private void Start()
        {
            data = GameFlowManager.Instance.Settings;

            if (masterSlider != null)
            {
                masterSlider.value = data.masterVolume;
                masterSlider.onValueChanged.AddListener(OnMasterChanged);
            }

            if (musicSlider != null)
            {
                musicSlider.value = data.musicVolume;
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = data.sfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = data.fullscreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }
        }

        private void OnMasterChanged(float value)
        {
            data.masterVolume = value;
            data.Apply();
        }

        private void OnMusicChanged(float value)
        {
            data.musicVolume = value;
        }

        private void OnSfxChanged(float value)
        {
            data.sfxVolume = value;
        }

        private void OnFullscreenChanged(bool value)
        {
            data.fullscreen = value;
        }

        private void OnBack()
        {
            data.Save();

            if (Closed != null)
            {
                Closed();
            }
            else
            {
                GameFlowManager.Instance.CloseSettings();
            }
        }
    }
}
