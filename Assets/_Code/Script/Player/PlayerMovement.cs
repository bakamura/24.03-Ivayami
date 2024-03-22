using Paranapiacaba.Puzzle;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Paranapiacaba.Player {
    public class PlayerMovement : MonoSingleton<PlayerMovement> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _movementInput;
        [SerializeField] private InputActionReference _crouchInput;

        [Header("Events")]

        public UnityEvent<float> onMovement = new UnityEvent<float>();
        public UnityEvent<bool> onCrouch = new UnityEvent<bool>();

        [Header("Movement")]

        [SerializeField] private float _movementSpeedMax;
        private float _speedCurrent = 0;
        [SerializeField] private float _accelerationDuration;
        private float _acceleration;
        [SerializeField] private float _deccelerationDuration;
        private float _decceleration;
        private bool _canMove;

        [Header("Rotation")]

        [SerializeField] private Transform _visualTransform;
        [Range(0f, 1f)]
        [SerializeField] private float _rotationSmoothFactor;

        [Header("Crouch")]

        [SerializeField] private float _crouchSpeedMax;
        private bool _crouching = false;
        [SerializeField] private float _walkColliderHeight;
        [SerializeField] private float _crouchColliderHeight;
        [SerializeField] private LayerMask _terrain;

        [Header("Camera")]

        [SerializeField] private Transform _cameraAimTargetRotator;

        [Header("Cache")]

        private Vector2 _inputCache = Vector3.zero;
        private Vector3 _movementDirectionCache = Vector3.zero;
        private float _movementSpeedMaxCurrent;
        private Quaternion _targetAngle;

        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private Transform _cameraTransform;

        protected override void Awake() {
            base.Awake();

            _movementInput.action.performed += MoveDirection;
            _movementInput.action.canceled += MoveDirection;
            _crouchInput.action.started += Crouch;

            _acceleration = Time.fixedDeltaTime / _accelerationDuration;
            _decceleration = Time.fixedDeltaTime / _deccelerationDuration;

            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _cameraTransform = Camera.main.transform; //

            Logger.Log(LogType.Player, $"{typeof(PlayerMovement).Name} Initialized");

            // Debug
            _canMove = true;
        }

        private void FixedUpdate() {
            Move();
            RotateCameraTarget();
        }

        private void MoveDirection(InputAction.CallbackContext input) {
            _inputCache = input.ReadValue<Vector2>();

            Logger.Log(LogType.Player, $"Movement Input Change: {input.ReadValue<Vector2>()}");
        }

        private void Move() {
            if (_canMove && _inputCache.sqrMagnitude > 0) {
                _movementDirectionCache[0] = _inputCache[0];
                _movementDirectionCache[2] = _inputCache[1];
                _movementSpeedMaxCurrent = _inputCache.magnitude * (_crouching ? _crouchSpeedMax : _movementSpeedMax);
                _targetAngle = Quaternion.Euler(0, Mathf.Atan2(_movementDirectionCache.x, _movementDirectionCache.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y, 0);
            }
            _speedCurrent = Mathf.Clamp(_speedCurrent + (_inputCache.sqrMagnitude > 0 ? _acceleration : -_decceleration), 0, _movementSpeedMaxCurrent);

            _rigidbody.velocity = (_targetAngle * Vector3.forward).normalized * _speedCurrent;
            _visualTransform.rotation = Quaternion.Slerp(_visualTransform.rotation, _targetAngle, _rotationSmoothFactor);

            onMovement.Invoke(_speedCurrent);
        }

        private void Crouch(InputAction.CallbackContext input) {
            if (!_crouching || !Physics.Raycast(transform.position, transform.up, _walkColliderHeight, _terrain)) {
                _crouching = !_crouching;
                _collider.height = _crouching ? _crouchColliderHeight : _walkColliderHeight;
                _collider.center = 0.5f * (_crouching ? _crouchColliderHeight : _walkColliderHeight) * Vector3.up;

                onCrouch.Invoke(_crouching);

                Logger.Log(LogType.Player, $"Crouch Toggle: {_crouching}");
            }
            else Logger.Log(LogType.Player, $"Crouch Toggle Fail: Terrain Above");
        }

        private void RotateCameraTarget() {
            _cameraAimTargetRotator.eulerAngles = _cameraTransform.eulerAngles.y * Vector3.up;
        }

        public void ToggleMovement(bool canMove) {
            _canMove = canMove;
            if (!_canMove) _inputCache = Vector2.zero;

            Logger.Log(LogType.Player, $"Movement Toggle: {_canMove}");
        }

    }
}