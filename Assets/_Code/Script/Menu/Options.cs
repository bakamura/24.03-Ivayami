using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ivayami.Audio;
using Ivayami.Save;
using TMPro;
using Ivayami.Player;
using Ivayami.Puzzle;

namespace Ivayami.UI {
    public class Options : MonoBehaviour {

        public static UnityEvent<LanguageTypes> OnChangeLanguage { get; private set; } = new UnityEvent<LanguageTypes>();

        [Header("UI")]

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Space(16)]

        [SerializeField] private Slider _cameraSensitivitySliderX;
        [SerializeField] private float _mouseCameraSensitivityMultiplierX;
        [SerializeField] private float _gamepadCameraSensitivityMultiplierX;
        [SerializeField] private Slider _cameraSensitivitySliderY;
        [SerializeField] private float _mouseCameraSensitivityMultiplierY;
        [SerializeField] private float _gamepadCameraSensitivityMultiplierY;

        [Space(16)]

        [SerializeField] private TMP_Dropdown _languageDropdown;

        private void Start() {
            StartCoroutine(StartRoutine());
        }

        private IEnumerator StartRoutine() {
            while (SaveSystem.Instance.Options == null) yield return null;

            ParametersApplySave();
            InputCallbacks.Instance.SubscribeToOnChangeControls(ControlSensitivityUpdate);
        }

        public void ChangeMusicVolume(float newVolume) {
            SaveSystem.Instance.Options.musicVol = newVolume;
            Music.Instance.VolumeUpdate(newVolume);
        }

        public void ChangeSfxVolume(float newVolume) {
            SaveSystem.Instance.Options.sfxVol = newVolume;
        }

        public void ChangeSensitivityX(float sensitivityX) {
            SaveSystem.Instance.Options.cameraSensitivityX = sensitivityX;
            PlayerCamera.Instance.SetSensitivityX(sensitivityX * (InputCallbacks.Instance.IsGamepad ? _gamepadCameraSensitivityMultiplierX : _mouseCameraSensitivityMultiplierX));
        }

        public void ChangeSensitivityY(float sensitivityY) {
            SaveSystem.Instance.Options.cameraSensitivityY = sensitivityY;
            PlayerCamera.Instance.SetSensitivityY(sensitivityY * (InputCallbacks.Instance.IsGamepad ? _gamepadCameraSensitivityMultiplierY :_mouseCameraSensitivityMultiplierY));
        }

        public void ChangeLanguage(Int32 language) {
            SaveSystem.Instance.Options.language = language;
            OnChangeLanguage.Invoke((LanguageTypes)language);
        }

        public void ParametersUpdate() {
            _musicSlider.value = SaveSystem.Instance.Options.musicVol;
            _sfxSlider.value = SaveSystem.Instance.Options.sfxVol;
            _cameraSensitivitySliderX.value = SaveSystem.Instance.Options.cameraSensitivityX;
            _cameraSensitivitySliderY.value = SaveSystem.Instance.Options.cameraSensitivityY;
            _languageDropdown.SetValueWithoutNotify(SaveSystem.Instance.Options.language);
        }

        public void ParametersApplySave() {
            ParametersUpdate();
            Music.Instance.VolumeUpdate(_musicSlider.value);
            PlayerCamera.Instance.SetSensitivityX(SaveSystem.Instance.Options.cameraSensitivityX * _mouseCameraSensitivityMultiplierX);
            PlayerCamera.Instance.SetSensitivityY(SaveSystem.Instance.Options.cameraSensitivityY * _mouseCameraSensitivityMultiplierY);
            ChangeLanguage(SaveSystem.Instance.Options.language);
        }

        private void ControlSensitivityUpdate(bool isGamepad) {
            PlayerCamera.Instance.SetSensitivityX(SaveSystem.Instance.Options.cameraSensitivityX * (isGamepad ? _gamepadCameraSensitivityMultiplierX : _mouseCameraSensitivityMultiplierX));
            PlayerCamera.Instance.SetSensitivityY(SaveSystem.Instance.Options.cameraSensitivityY * (isGamepad ? _gamepadCameraSensitivityMultiplierY : _mouseCameraSensitivityMultiplierY));
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

    }
}
