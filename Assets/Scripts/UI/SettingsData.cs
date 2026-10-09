using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Datos de ajustes del usuario persistidos en PlayerPrefs (volúmenes y pantalla completa).
    /// </summary>
    [System.Serializable]
    public class SettingsData
    {
        private const string MasterKey = "md.settings.master";
        private const string MusicKey = "md.settings.music";
        private const string SfxKey = "md.settings.sfx";
        private const string FullscreenKey = "md.settings.fullscreen";

        /// <summary>
        /// Volumen maestro (0..1); se aplica a AudioListener.volume.
        /// </summary>
        public float masterVolume = 0.8f;
        /// <summary>
        /// Volumen de la música (0..1).
        /// </summary>
        public float musicVolume = 0.8f;
        /// <summary>
        /// Volumen de los efectos de sonido (0..1).
        /// </summary>
        public float sfxVolume = 0.8f;
        /// <summary>
        /// Indica si se juega a pantalla completa.
        /// </summary>
        public bool fullscreen = true;

        /// <summary>
        /// Carga los ajustes desde PlayerPrefs (usando los valores por defecto si no existen) y devuelve una instancia.
        /// </summary>
        public static SettingsData Load()
        {
            SettingsData data = new SettingsData();
            data.masterVolume = PlayerPrefs.GetFloat(MasterKey, data.masterVolume);
            data.musicVolume = PlayerPrefs.GetFloat(MusicKey, data.musicVolume);
            data.sfxVolume = PlayerPrefs.GetFloat(SfxKey, data.sfxVolume);
            data.fullscreen = PlayerPrefs.GetInt(FullscreenKey, data.fullscreen ? 1 : 0) == 1;
            return data;
        }

        /// <summary>
        /// Guarda los ajustes en PlayerPrefs.
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetFloat(MasterKey, masterVolume);
            PlayerPrefs.SetFloat(MusicKey, musicVolume);
            PlayerPrefs.SetFloat(SfxKey, sfxVolume);
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Aplica los ajustes al motor (volumen maestro y pantalla completa).
        /// </summary>
        public void Apply()
        {
            AudioListener.volume = masterVolume;
            Screen.fullScreen = fullscreen;
        }
    }
}
