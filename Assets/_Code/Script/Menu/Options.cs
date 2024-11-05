using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ivayami.Save;
using TMPro;
using Ivayami.Player;
using Ivayami.Puzzle;
using FMOD.Studio;
using FMODUnity;

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

        [Space(16)]

        [SerializeField] private Toggle _invertCameraToggle;

        [Space(16)]
        [SerializeField] private Slider _deadzoneSlider;

        public static Bus Music { get; private set; }
        public static Bus Sfx { get; private set; }
        public static Bus GameplaySfx { get; private set; }
        //private Bus _master;

        private void Awake()
        {
            //_master = RuntimeManager.GetBus("bus:/Master");
            Music = RuntimeManager.GetBus("bus:/Master/Music");
            Sfx = RuntimeManager.GetBus("bus:/Master/SFX_Geral");
            GameplaySfx = RuntimeManager.GetBus("bus:/Master/SFX_Geral/SFX");
        }

        private void Start() {
            StartCoroutine(StartRoutine());
        }

        private IEnumerator StartRoutine() {
            while (SaveSystem.Instance.Options == null) yield return null;

            ParametersApplySave();
            InputCallbacks.Instance.SubscribeToOnChangeControls(ControlSensitivityUpdate);
        }

        public void ChangeMusicVolume(float newVolume) {
            Music.setVolume(newVolume);
            SaveSystem.Instance.Options.musicVol = newVolume;
        }

        public void ChangeSfxVolume(float newVolume) {
            Sfx.setVolume(newVolume);
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
            Music.setVolume(_musicSlider.value);
            _sfxSlider.value = SaveSystem.Instance.Options.sfxVol;
            Sfx.setVolume(_sfxSlider.value);
            _cameraSensitivitySliderX.value = SaveSystem.Instance.Options.cameraSensitivityX;
            _cameraSensitivitySliderY.value = SaveSystem.Instance.Options.cameraSensitivityY;
            _languageDropdown.SetValueWithoutNotify(SaveSystem.Instance.Options.language);
            _invertCameraToggle.isOn = SaveSystem.Instance.Options.invertCamera;
            _deadzoneSlider.value = SaveSystem.Instance.Options.cameraDeadzone;
        }

        public void ParametersApplySave() {
            ParametersUpdate();
            PlayerCamera.Instance.SetSensitivityX(SaveSystem.Instance.Options.cameraSensitivityX * _mouseCameraSensitivityMultiplierX);
            PlayerCamera.Instance.SetSensitivityY(SaveSystem.Instance.Options.cameraSensitivityY * _mouseCameraSensitivityMultiplierY);
            ChangeLanguage(SaveSystem.Instance.Options.language);
            InvertCamera(SaveSystem.Instance.Options.invertCamera);
            PlayerMovement.Instance.ChangeStickDeadzone(SaveSystem.Instance.Options.cameraDeadzone);
        }

        private void ControlSensitivityUpdate(bool isGamepad) {
            PlayerCamera.Instance.SetSensitivityX(SaveSystem.Instance.Options.cameraSensitivityX * (isGamepad ? _gamepadCameraSensitivityMultiplierX : _mouseCameraSensitivityMultiplierX));
            PlayerCamera.Instance.SetSensitivityY(SaveSystem.Instance.Options.cameraSensitivityY * (isGamepad ? _gamepadCameraSensitivityMultiplierY : _mouseCameraSensitivityMultiplierY));
        }

        public void InvertCamera(bool isActive)
        {
            SaveSystem.Instance.Options.invertCamera = isActive;
            PlayerCamera.Instance.InvertCamera(!isActive);
        }

        public void ChangeCameraDeadzone(float deadzoneRange)
        {
            SaveSystem.Instance.Options.cameraDeadzone = deadzoneRange;
            PlayerMovement.Instance.ChangeStickDeadzone(deadzoneRange);
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

    }
}
