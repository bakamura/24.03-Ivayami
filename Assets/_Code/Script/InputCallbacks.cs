using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.iOS;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

namespace Ivayami.Player {
    public class InputCallbacks : MonoSingleton<InputCallbacks> {

        private UnityEvent<bool> _onChangeControls = new UnityEvent<bool>();

        private PlayerInput _playerInput;
        public bool IsGamepad { get; private set; }
        public GamepadType GamepadCurrent { get; private set; }

        public enum GamepadType {
            NotGamepad,
            XBox,
            DualShock,
            DualSense,
            ProController
        }

        protected override void Awake() {
            base.Awake();

            InputSystem.onDeviceChange += (device, deviceChange) => {
                IsGamepad = device is Gamepad;
                GamepadCurrent = IsGamepad ? GetGamepadType(Gamepad.current) : GamepadType.NotGamepad;
                Debug.Log($"Gamepad current is '{GamepadCurrent.ToString()}'");
            };
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.onControlsChanged += (playerInput) => {
                _onChangeControls.Invoke(playerInput.currentControlScheme == "Gamepad");
            };
        }

        /// <summary>
        /// Callback 'true' when 'isGamepad == true'
        /// </summary>
        public void SubscribeToOnChangeControls(UnityAction<bool> action) {
            _onChangeControls.AddListener(action);
            action.Invoke(_playerInput.currentControlScheme == "Gamepad");
        }

        public void UnsubscribeToOnChangeControls(UnityAction<bool> action) {
            _onChangeControls.RemoveListener(action);
        }

        public GamepadType GetGamepadType(Gamepad gamepad) {
            if (gamepad is XInputController) return GamepadType.XBox;
            if (gamepad is DualShockGamepad) return GamepadType.DualShock;
            if (gamepad is DualSenseGamepadHID || gamepad is DualSenseGampadiOS) return GamepadType.DualSense;
            if (gamepad is SwitchProControllerHID) return GamepadType.ProController;
            Debug.LogError($"Non-identified Gamepad '{gamepad.name}'");
            return GamepadType.DualShock;
        }

    }
}