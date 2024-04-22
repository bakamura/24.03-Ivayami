using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.Player {
    public class PlayerMovement : MonoSingleton<PlayerMovement> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _movementInput;
        [SerializeField] private InputActionReference _crouchInput;

        [Header("Events")]

        public UnityEvent<Vector2> onMovement = new UnityEvent<Vector2>();
        public UnityEvent<bool> onCrouch = new UnityEvent<bool>();

        [Header("Movement")]

        [SerializeField] private float _movementSpeedMax;
        private float _speedCurrent = 0;
        [SerializeField] private float _accelerationDuration;
        private float _acceleration;
        [SerializeField] private float _deccelerationDuration;
        private float _decceleration;
        [SerializeField, Range(0f, 1f)] private float _movementSpeedBackwardsMultiplier;
        [SerializeField, Range(0f, 180f)] private float _movementBacwardsAngleMaxFromForward;
        private bool _canMove;

        [Header("Rotation")]

        [SerializeField] private Transform _visualTransform;

        [Header("Crouch")]

        [SerializeField] private float _crouchSpeedMax;
        public bool Crouching { get; private set; } = false;
        [SerializeField] private float _walkColliderHeight;
        [SerializeField] private float _walkCameraHeight;
        [SerializeField] private float _crouchColliderHeight;
        [SerializeField] private float _crouchCameraHeight;
        [SerializeField] private LayerMask _terrain;

        [Header("Camera")]

        [SerializeField] private Transform _cameraAimTargetRotator;

        [Header("Cache")]

        private Vector2 _inputCache = Vector3.zero;
        private Vector3 _movementDirectionCache = Vector3.zero;
        private Vector3 _velocityCache;
        private float _movementSpeedMaxCurrent;
        private Quaternion _targetAngle;
        private float _directionDifferenceToInputAngleCache;

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
        }

        private void Update() {
            if (_canMove) Rotate();
        }

        private void FixedUpdate() {
            if(_canMove) Move();
        }

        private void MoveDirection(InputAction.CallbackContext input) {
            _inputCache = input.ReadValue<Vector2>();

            Logger.Log(LogType.Player, $"Movement Input Change: {input.ReadValue<Vector2>()}");
        }

        private void Move() {
            if (_inputCache.sqrMagnitude > 0) {
                _movementDirectionCache[0] = _inputCache[0];
                _movementDirectionCache[2] = _inputCache[1];
                _targetAngle = Quaternion.Euler(0, Mathf.Atan2(_movementDirectionCache.x, _movementDirectionCache.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y, 0);
                _directionDifferenceToInputAngleCache = Mathf.Abs(_targetAngle.eulerAngles.y - _cameraAimTargetRotator.eulerAngles.y);
                if (_directionDifferenceToInputAngleCache > 180) _directionDifferenceToInputAngleCache -= 180f;
                _movementSpeedMaxCurrent = _movementDirectionCache.magnitude 
                                         * (Crouching ? _crouchSpeedMax : _movementSpeedMax) 
                                         * Mathf.Lerp(1f, _movementSpeedBackwardsMultiplier, _directionDifferenceToInputAngleCache / _movementBacwardsAngleMaxFromForward);
            }
            _speedCurrent = Mathf.Clamp(_speedCurrent + (_inputCache.sqrMagnitude > 0 ? _acceleration : -_decceleration), 0, _movementSpeedMaxCurrent); // Could use _decceleration when above max speed

            _velocityCache = _rigidbody.velocity;
            _rigidbody.velocity = (_targetAngle * Vector3.forward).normalized * _speedCurrent;
            _velocityCache[0] = _rigidbody.velocity.x;
            _velocityCache[2] = _rigidbody.velocity.z;
            _rigidbody.velocity = _velocityCache;

            onMovement?.Invoke(_speedCurrent * _inputCache);
        }

        private void Crouch(InputAction.CallbackContext input) {
            if (!Physics.Raycast(transform.position, transform.up, _walkColliderHeight, _terrain)) {
                Crouching = !Crouching;
                _collider.height = Crouching ? _crouchColliderHeight : _walkColliderHeight;
                _collider.center = 0.5f * (Crouching ? _crouchColliderHeight : _walkColliderHeight) * Vector3.up;

                _cameraAimTargetRotator.localPosition = (Crouching ? _crouchCameraHeight : _walkCameraHeight) * Vector3.up;

                onCrouch?.Invoke(Crouching);

                Logger.Log(LogType.Player, $"Crouch Toggle: {Crouching}");
            }
            else Logger.Log(LogType.Player, $"Crouch Toggle Fail: Terrain Above");
        }

        private void Rotate() {
            _cameraAimTargetRotator.eulerAngles = _cameraTransform.eulerAngles.y * Vector3.up;
            _visualTransform.eulerAngles = _cameraAimTargetRotator.eulerAngles;
        }

        public void ToggleMovement(bool canMove) {
            _canMove = canMove;
            if (!_canMove) _speedCurrent = 0f;

            Logger.Log(LogType.Player, $"Movement Toggle: {_canMove}");
        }

        public void DisableMovement(float duration) {
            StartCoroutine(DisableMovementRoutine(duration));
        }

        private IEnumerator DisableMovementRoutine(float duration) {
            _canMove = false; 
            
            yield return new WaitForSeconds(duration);

            _canMove = true;
        }

    }
}