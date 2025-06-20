using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.Localization.Settings;
using TMPro;
using FMOD.Studio;
using FMODUnity;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.UI {
    public class Options : MonoBehaviour {

        public static UnityEvent OnChangeLanguage { get; private set; } = new UnityEvent();

        [Header("UI")]

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;        
        [SerializeField] private Slider _rightStickDeadzoneSlider;
        [SerializeField] private Slider _leftStickDeadzoneSlider;
        [SerializeField] private Slider _brightnessSlider;

        [Space(16)]

        [SerializeField] private Slider _cameraSensitivitySliderX;
        [SerializeField] private float _mouseCameraSensitivityMultiplierX;
        [SerializeField] private float _gamepadCameraSensitivityMultiplierX;
        [SerializeField] private Slider _cameraSensitivitySliderY;
        [SerializeField] private float _mouseCameraSensitivityMultiplierY;
        [SerializeField] private float _gamepadCameraSensitivityMultiplierY;
        [SerializeField] private InputActionReference[] _leftStickDeadzoneInputs;
        [SerializeField] private InputActionReference[] _rightStickDeadzoneInputs;

        [Space(16)]

        [SerializeField] private TMP_Text _languageNameText;
        //[SerializeField] private TMP_Dropdown _languageDropdown;
        private byte _languageTypesSize;

        [Space(16)]

        [SerializeField] private Toggle _invertCameraToggle;
        [SerializeField] private Toggle _holdToRunToggle;

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
            _languageTypesSize = (byte)LocalizationSettings.AvailableLocales.Locales.Count;
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

        public void ChangeLanguage(int value)
        {
            SaveSystem.Instance.Options.language += value;
            if (SaveSystem.Instance.Options.language >= _languageTypesSize) SaveSystem.Instance.Options.language = 0;
            else if (SaveSystem.Instance.Options.language < 0) SaveSystem.Instance.Options.language = _languageTypesSize - 1;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[SaveSystem.Instance.Options.language];
            _languageNameText.text = LocalizationSettings.AvailableLocales.Locales[SaveSystem.Instance.Options.language].LocaleName;
            OnChangeLanguage.Invoke();
        }        

        //public void ChangeLanguage(Int32 language) {
        //    SaveSystem.Instance.Options.language = language;
        //    OnChangeLanguage.Invoke((LanguageTypes)language);
        //}

        public void ParametersUpdate() {
            _musicSlider.SetValueWithoutNotify(SaveSystem.Instance.Options.musicVol);
            _sfxSlider.SetValueWithoutNotify(SaveSystem.Instance.Options.sfxVol);
            _cameraSensitivitySliderX.SetValueWithoutNotify(SaveSystem.Instance.Options.cameraSensitivityX);
            _cameraSensitivitySliderY.SetValueWithoutNotify(SaveSystem.Instance.Options.cameraSensitivityY);
            _rightStickDeadzoneSlider.SetValueWithoutNotify(SaveSystem.Instance.Options.cameraDeadzone);
            _leftStickDeadzoneSlider.SetValueWithoutNotify(SaveSystem.Instance.Options.movementDeadzone);
            _brightnessSlider.SetValueWithoutNotify(SaveSystem.Instance.Options.brightness);
            _languageNameText.text = LocalizationSettings.AvailableLocales.Locales[SaveSystem.Instance.Options.language].LocaleName;
            //_languageDropdown.SetValueWithoutNotify(SaveSystem.Instance.Options.language);
            _invertCameraToggle.SetIsOnWithoutNotify(SaveSystem.Instance.Options.invertCamera);
            _holdToRunToggle.SetIsOnWithoutNotify(SaveSystem.Instance.Options.holdToRun);
        }

        private void ParametersApplySave() {
            ParametersUpdate();
            Music.setVolume(_musicSlider.value);
            Sfx.setVolume(_sfxSlider.value);
            PlayerCamera.Instance.SetSensitivityX(SaveSystem.Instance.Options.cameraSensitivityX * (InputCallbacks.Instance.IsGamepad ? _gamepadCameraSensitivityMultiplierX : _mouseCameraSensitivityMultiplierX));
            PlayerCamera.Instance.SetSensitivityY(SaveSystem.Instance.Options.cameraSensitivityY * (InputCallbacks.Instance.IsGamepad ? _gamepadCameraSensitivityMultiplierY : _mouseCameraSensitivityMultiplierY));
            PostProcessManager.Instance.ChangeBrightness(SaveSystem.Instance.Options.brightness);
            //PlayerMovement.Instance.ChangeStickDeadzone(SaveSystem.Instance.Options.cameraDeadzone);
            UpdateRightStickDeadzones(SaveSystem.Instance.Options.cameraDeadzone);
            UpdateLeftStickDeadzones(SaveSystem.Instance.Options.movementDeadzone);
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[SaveSystem.Instance.Options.language];
            OnChangeLanguage.Invoke();
            PlayerCamera.Instance.InvertCamera(!SaveSystem.Instance.Options.invertCamera);
            PlayerMovement.Instance.ChangeHoldToRun(SaveSystem.Instance.Options.holdToRun);
        }

        private void ControlSensitivityUpdate(InputCallbacks.ControlType controlType) {
            PlayerCamera.Instance.SetSensitivityX(SaveSystem.Instance.Options.cameraSensitivityX * (InputCallbacks.Instance.IsGamepad ? _gamepadCameraSensitivityMultiplierX : _mouseCameraSensitivityMultiplierX));
            PlayerCamera.Instance.SetSensitivityY(SaveSystem.Instance.Options.cameraSensitivityY * (InputCallbacks.Instance.IsGamepad ? _gamepadCameraSensitivityMultiplierY : _mouseCameraSensitivityMultiplierY));
        }

        public void InvertCamera(bool isActive)
        {
            SaveSystem.Instance.Options.invertCamera = isActive;
            PlayerCamera.Instance.InvertCamera(!isActive);
        }

        public void ChangeCameraDeadzone(float deadzoneRange)
        {
            SaveSystem.Instance.Options.cameraDeadzone = deadzoneRange;
            UpdateRightStickDeadzones(deadzoneRange);
            //PlayerMovement.Instance.ChangeStickDeadzone(deadzoneRange);
        }

        public void ChangeMovementDeadzone(float deadzoneRange)
        {
            SaveSystem.Instance.Options.movementDeadzone = deadzoneRange;
            UpdateLeftStickDeadzones(deadzoneRange);
        }

        private void UpdateLeftStickDeadzones(float deadzoneRange)
        {
            for (int i = 0; i < _leftStickDeadzoneInputs.Length; i++)
            {
                _leftStickDeadzoneInputs[i].action.ApplyParameterOverride((StickDeadzoneProcessor s) => s.min, deadzoneRange);
            }
        }

        private void UpdateRightStickDeadzones(float deadzoneRange)
        {
            for (int i = 0; i < _rightStickDeadzoneInputs.Length; i++)
            {
                _rightStickDeadzoneInputs[i].action.ApplyParameterOverride((StickDeadzoneProcessor s) => s.min, deadzoneRange);
            }
        }

        public void ToggleToRun(bool isActive)
        {
            SaveSystem.Instance.Options.holdToRun = isActive;
            PlayerMovement.Instance.ChangeHoldToRun(isActive);
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

        public void ChangeBrightness(float value)
        {
            SaveSystem.Instance.Options.brightness = value;
            PostProcessManager.Instance.ChangeBrightness(value);
        }

        public void ToggleCanPause(bool canPause) {
            Pause.Instance.ToggleCanPause(nameof(Options), canPause);
        }

    }
}
