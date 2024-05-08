using Ivayami.Audio;
using Ivayami.Save;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class Options : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        private void Start() {
            StartCoroutine(StartRoutine());
        }

        private IEnumerator StartRoutine() {
            while (SaveSystem.Instance.Progress == null) yield return null;

            _musicSlider.value = SaveSystem.Instance.Options.musicVol;
            _sfxSlider.value = SaveSystem.Instance.Options.sfxVol;
            Music.Instance.SetVolume(_musicSlider.value);
        }

        public void ChangeMusicVolume(float newVolume) {
            SaveSystem.Instance.Options.musicVol = newVolume;
            Music.Instance.SetVolume(newVolume);
        }

        public void ChangeSfxVolume(float newVolume) {
            SaveSystem.Instance.Options.sfxVol = newVolume;
        }

        public void SliderUpdate() {
            _musicSlider.value = SaveSystem.Instance.Options.musicVol;
            _sfxSlider.value = SaveSystem.Instance.Options.sfxVol;
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

    }
}
