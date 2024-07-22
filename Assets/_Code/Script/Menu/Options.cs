using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ivayami.Audio;
using Ivayami.Save;

namespace Ivayami.UI {
    public class Options : MonoBehaviour {

        public static UnityEvent<LanguageTypes> OnChangeLanguage { get; private set; } = new UnityEvent<LanguageTypes>();

        [Header("UI")]

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _cameraSensitivitySlider;

        private void Start() {
            StartCoroutine(StartRoutine());
        }

        private IEnumerator StartRoutine() {
            while (SaveSystem.Instance.Options == null) yield return null;

            SliderUpdate();
            Music.Instance.VolumeUpdate(_musicSlider.value);
            ChangeLanguage(SaveSystem.Instance.Options.language);
        }

        public void ChangeMusicVolume(float newVolume) {
            SaveSystem.Instance.Options.musicVol = newVolume;
            Music.Instance.VolumeUpdate(newVolume);
        }

        public void ChangeSfxVolume(float newVolume) {
            SaveSystem.Instance.Options.sfxVol = newVolume;
        }

        public void SliderUpdate() {
            _musicSlider.value = SaveSystem.Instance.Options.musicVol;
            _sfxSlider.value = SaveSystem.Instance.Options.sfxVol;
            if (_cameraSensitivitySlider != null) _cameraSensitivitySlider.value = SaveSystem.Instance.Options.cameraSensitivity;
        }

        public void ChangeLanguage(Int32 language) {
            SaveSystem.Instance.Options.language = language;
            OnChangeLanguage.Invoke((LanguageTypes)language);
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

    }
}
