using Paranapiacaba.Audio;
using Paranapiacaba.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Paranapiacaba.UI {
    public class Options : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        private void Start() {
            Debug.Log($"Save Instance {(SaveSystem.Instance ? "exists" : "doesnt exist")}");
            _musicSlider.value = SaveSystem.Instance.Options.musicVol;
            _sfxSlider.value = SaveSystem.Instance.Options.sfxVol;
        }

        public void ChangeMusicVolume(float newVolume) {
            SaveSystem.Instance.Options.musicVol = newVolume;
            Music.Instance.SetVolume(newVolume);
        }

        public void ChangeSfxVolume(float newVolume) {
            SaveSystem.Instance.Options.sfxVol = newVolume;
            //EntitySound.SetVolume(newVolume);
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

    }
}
