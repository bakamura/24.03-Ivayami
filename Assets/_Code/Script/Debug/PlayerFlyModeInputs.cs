using UnityEngine;
using UnityEngine.InputSystem;
using Ivayami.Player;

namespace Ivayami.debug
{
    public class PlayerFlyModeInputs : MonoSingleton<PlayerFlyModeInputs>
    {
        [SerializeField] private InputActionReference _flyUp;
        [SerializeField] private InputActionReference _flyDown;
        private const float _currentSpeed = 5f;
        public void UpdateInputs(bool isActive/*, float speed*/)
        {
            //if (speed > 0) _currentSpeed = speed;
            if (isActive)
            {
                _flyUp.action.started += HandleFlyUpInputStart;
                _flyUp.action.canceled += HandleFlyUpInputStop;
                _flyDown.action.started += HandleFlyDownInputStart;
                _flyDown.action.canceled += HandleFlyDownInputStop;
            }
            else
            {
                _flyUp.action.started -= HandleFlyUpInputStart;
                _flyUp.action.canceled -= HandleFlyUpInputStop;
                _flyDown.action.started -= HandleFlyDownInputStart;
                _flyDown.action.canceled -= HandleFlyDownInputStop;
                PlayerMovement.Instance.UpdatePlayerGravity(1, 1);
            }
        }

        private void HandleFlyUpInputStart(InputAction.CallbackContext input)
        {
            PlayerMovement.Instance.UpdatePlayerGravity(1, _currentSpeed);
        }

        private void HandleFlyDownInputStart(InputAction.CallbackContext input)
        {
            PlayerMovement.Instance.UpdatePlayerGravity(-1, _currentSpeed);
        }

        private void HandleFlyUpInputStop(InputAction.CallbackContext input)
        {
            PlayerMovement.Instance.UpdatePlayerGravity(0, _currentSpeed);
        }

        private void HandleFlyDownInputStop(InputAction.CallbackContext input)
        {
            PlayerMovement.Instance.UpdatePlayerGravity(0, _currentSpeed);
        }
    }
}