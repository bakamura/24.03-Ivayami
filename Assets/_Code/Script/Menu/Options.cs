using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ivayami.Save;
using TMPro;
using Ivayami.Player;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

namespace Ivayami.UI {
    public class Options : MonoBehaviour {

        public static UnityEvent OnChangeLanguage { get; private set; } = new UnityEvent();

        [Header("UI")]

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;        
        [SerializeField] private Slider _rightStickDeadzoneSlider;
        [SerializeField] private Slider _leftStickDeadzoneSlider;

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
                Debug.Log(_leftStickDeadzoneInputs[i].action.GetParameterValue((StickDeadzoneProcessor s) => s.min));
            }
        }

        private void UpdateRightStickDeadzones(float deadzoneRange)
        {
            InputActionReference cameraInput = null;
            for (int i = 0; i < _rightStickDeadzoneInputs.Length; i++)
            {
                _rightStickDeadzoneInputs[i].action.ApplyParameterOverride((StickDeadzoneProcessor s) => s.min, deadzoneRange);
                if (string.Equals(_rightStickDeadzoneInputs[i].name, "Camera")) cameraInput = _rightStickDeadzoneInputs[i];
            }
            //PlayerCamera.Instance.UpdateCameraDeadzone(cameraInput);
        }

        public void ToggleToRun(bool isActive)
        {
            SaveSystem.Instance.Options.holdToRun = isActive;
            PlayerMovement.Instance.ChangeHoldToRun(isActive);
        }

        public void SaveOptions() {
            SaveSystem.Instance.SaveOptions();
        }

    }
}
