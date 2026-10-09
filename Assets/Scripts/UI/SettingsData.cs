using UnityEngine;

namespace MadnessDreams
{
    [System.Serializable]
    public class SettingsData
    {
        private const string MasterKey = "md.settings.master";
        private const string MusicKey = "md.settings.music";
        private const string SfxKey = "md.settings.sfx";
        private const string FullscreenKey = "md.settings.fullscreen";

        public float masterVolume = 0.8f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public bool fullscreen = true;

        public static SettingsData Load()
        {
            SettingsData data = new SettingsData();
            data.masterVolume = PlayerPrefs.GetFloat(MasterKey, data.masterVolume);
            data.musicVolume = PlayerPrefs.GetFloat(MusicKey, data.musicVolume);
            data.sfxVolume = PlayerPrefs.GetFloat(SfxKey, data.sfxVolume);
            data.fullscreen = PlayerPrefs.GetInt(FullscreenKey, data.fullscreen ? 1 : 0) == 1;
            return data;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(MasterKey, masterVolume);
            PlayerPrefs.SetFloat(MusicKey, musicVolume);
            PlayerPrefs.SetFloat(SfxKey, sfxVolume);
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Apply()
        {
            AudioListener.volume = masterVolume;
            Screen.fullScreen = fullscreen;
        }
    }
}
