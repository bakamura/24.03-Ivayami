using UnityEngine;
using UnityEngine.InputSystem;

namespace Paranapiacaba.Player {
    public class PlayerMovement : MonoSingleton<PlayerMovement> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _movementInput;

        [Header("Parameters")]

        [SerializeField] private float _movementSpeedMax;
        [SerializeField] private float _accelerationDuration;
        [SerializeField] private float _deccelerationDuration;
        private float _speedCurrent;

        [Header("Cache")]

        private Vector2 _inputCache = Vector3.zero;
        private float _movementSpeedMaxCurrent;
        private float _acceleration;
        private float _decceleration;

        private Rigidbody _rigidbody;

        protected override void Awake() {
            base.Awake();

            _movementInput.action.performed += Move;

            _rigidbody = GetComponent<Rigidbody>();

            Logger.Log(LogType.Player, $"{typeof(PlayerMovement).Name} Initialized");
        }

        private void Move(InputAction.CallbackContext input) {
            _inputCache = input.ReadValue<Vector2>();
            _movementSpeedMaxCurrent = _inputCache.magnitude * _movementSpeedMax;

            _speedCurrent = Mathf.Clamp(_speedCurrent + (_speedCurrent < _movementSpeedMaxCurrent ? _acceleration : -_decceleration), 0, _movementSpeedMaxCurrent);
            _rigidbody.velocity = _inputCache.normalized * _speedCurrent;
        }

    }
}