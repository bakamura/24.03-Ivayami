using System;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.LowLevel;
//using UnityEngine.InputSystem.Users;

namespace Ivayami.Puzzle
{
    public class InputCallbacks : MonoSingleton<InputCallbacks>
    {
        private PlayerInput _playerInput
        {
            get
            {
                if (!m_playerInput) m_playerInput = GetComponent<PlayerInput>();
                return m_playerInput;
            }
        }
        private PlayerInput m_playerInput;
        public string CurrentControlScheme => _playerInput.currentControlScheme;

        public void AddEventToOnChangeControls(Action<PlayerInput> action)
        {
            _playerInput.onControlsChanged += action;
        }

        public void RemoveEventToOnChangeControls(Action<PlayerInput> action)
        {
            _playerInput.onControlsChanged -= action;
        }
        #region Old Version
        // Subscribe to this event
        //public event Action<ControlScheme> OnInputSchemeChanged;
        //public ControlScheme CurrentControlScheme { get; private set; }
        //public InputActionAsset inputActions;

        //public InputUser user;
        //private void Start() => StartAutoControlSchemeSwitching();
        //private void OnDestroy() => StopAutoControlSchemeSwitching();        

        //void StartAutoControlSchemeSwitching()
        //{
        //    user = InputUser.CreateUserWithoutPairedDevices();
        //    user.AssociateActionsWithUser(inputActions.actionMaps[0]); // need to be there at least one actionmap defined in InputActionAsset, otherwise rises exception during paring process
        //    ++InputUser.listenForUnpairedDeviceActivity;
        //    InputUser.onUnpairedDeviceUsed += InputUser_onUnpairedDeviceUsed;
        //    user.UnpairDevices();
        //}

        //private void StopAutoControlSchemeSwitching()
        //{
        //    InputUser.onUnpairedDeviceUsed -= InputUser_onUnpairedDeviceUsed;
        //    if (InputUser.listenForUnpairedDeviceActivity > 0)
        //        --InputUser.listenForUnpairedDeviceActivity;
        //    user.UnpairDevicesAndRemoveUser();
        //}

        //private void InputUser_onUnpairedDeviceUsed(InputControl ctrl, UnityEngine.InputSystem.LowLevel.InputEventPtr eventPtr)
        //{
        //    var device = ctrl.device;

        //    if ((CurrentControlScheme == ControlScheme.KeyboardMouse) &&
        //         ((device is Pointer) || (device is Keyboard)))
        //    {
        //        InputUser.PerformPairingWithDevice(device, user);
        //        if (OnInputSchemeChanged != null) OnInputSchemeChanged(ControlScheme.KeyboardMouse);
        //        SetUserControlScheme(ControlScheme.KeyboardMouse);
        //        return;
        //    }

        //    if (device is Gamepad)
        //    {
        //        if (OnInputSchemeChanged != null) OnInputSchemeChanged(ControlScheme.Gamepad);
        //        CurrentControlScheme = ControlScheme.Gamepad;
        //        SetUserControlScheme(ControlScheme.Gamepad);
        //    }
        //    else if ((device is Keyboard) || (device is Pointer))
        //    {
        //        if (OnInputSchemeChanged != null) OnInputSchemeChanged(ControlScheme.KeyboardMouse);
        //        CurrentControlScheme = ControlScheme.KeyboardMouse;
        //        SetUserControlScheme(ControlScheme.KeyboardMouse);
        //    }
        //    else return;

        //    user.UnpairDevices();
        //    InputUser.PerformPairingWithDevice(device, user);
        //}
        //public void SetUserControlScheme(ControlScheme scheme)
        //{
        //    //user.ActivateControlScheme(scheme.ToString());
        //    user.ActivateControlScheme(inputActions.controlSchemes[(int)scheme]); // this should be faster and not vulnerable to scheme string names
        //}
        //public enum ControlScheme
        //{
        //    KeyboardMouse = 0, Gamepad = 1 // just need to be same indexes as defined in inputActionAsset
        //}
        #endregion
    }
}