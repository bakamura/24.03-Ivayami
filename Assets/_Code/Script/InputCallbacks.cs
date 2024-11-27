using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.Player {
    public class InputCallbacks : MonoSingleton<InputCallbacks> {

        private UnityEvent<bool> _onChangeControls = new UnityEvent<bool>();

        private PlayerInput _playerInput;
        public bool IsGamepad { get { return _playerInput.currentControlScheme == "Gamepad"; } }
        public GamepadType GamepadCurrent { get; private set; }

        public enum GamepadType {
            NotGamepad,
            XBox,
            DualShock,
            DualSense
        }

        protected override void Awake() {
            base.Awake();

            _playerInput = GetComponent<PlayerInput>();
            _playerInput.onControlsChanged += (playerInput) => {
                GamepadCurrent = IsGamepad ? StringToGamepadType(Gamepad.current.displayName) : GamepadType.NotGamepad;
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

        public GamepadType StringToGamepadType(string str) {
            str = str.ToLower();
            Debug.Log($"Gamepad named '{str}' connected");
            if (str.Contains("xbox")) return GamepadType.XBox;
            if (str.Contains("dualshock")) return GamepadType.DualShock;
            if (str.Contains("dualsense")) return GamepadType.DualSense;
            Debug.LogError($"Non-identified Gamepad '{str}'");
            return GamepadType.XBox;
        }

    }
}