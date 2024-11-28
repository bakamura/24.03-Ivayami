using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.iOS;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

namespace Ivayami.Player {
    public class InputCallbacks : MonoSingleton<InputCallbacks> {

        private UnityEvent<ControlType> _onChangeControls = new UnityEvent<ControlType>();

        private PlayerInput _playerInput;
        public ControlType ControlTypeCurrent { get; private set; }
        public bool IsGamepad { get { return ControlTypeCurrent != ControlType.Keyboard; } }

        public enum ControlType {
            Keyboard,
            GamepadNotIdentified,
            XBox,
            DualShock,
            DualSense,
            ProController
        }

        protected override void Awake() {
            base.Awake();

            //InputSystem.onDeviceChange += (device, deviceChange) => {
            //};
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.onControlsChanged += (playerInput) => {
                ControlTypeCurrent = GetControlType(playerInput.currentControlScheme != "Gamepad" ? null : Gamepad.current);
                _onChangeControls.Invoke(ControlTypeCurrent);
                
                Debug.Log($"Control type is '{ControlTypeCurrent}'");
            };
        }

        /// <summary>
        /// Callback 'true' when 'isGamepad == true'
        /// </summary>
        public void SubscribeToOnChangeControls(UnityAction<ControlType> action) {
            _onChangeControls.AddListener(action);
            action.Invoke(ControlTypeCurrent);
        }

        public void UnsubscribeToOnChangeControls(UnityAction<ControlType> action) {
            _onChangeControls.RemoveListener(action);
        }

        public ControlType GetControlType(Gamepad gamepad) {
            if (!(gamepad is Gamepad)) return ControlType.Keyboard;
            if (gamepad is XInputController) return ControlType.XBox;
            if (gamepad is DualShockGamepad) return ControlType.DualShock;
            if (gamepad is DualSenseGamepadHID || gamepad is DualSenseGampadiOS) return ControlType.DualSense;
            if (gamepad is SwitchProControllerHID) return ControlType.ProController;
            Debug.LogWarning($"Non-identified Gamepad '{gamepad.name}'");
            return ControlType.GamepadNotIdentified;
        }

    }
}