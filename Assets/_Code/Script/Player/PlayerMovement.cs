using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.Player {
    public class PlayerMovement : MonoSingleton<PlayerMovement> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _movementInput;
        [SerializeField] private InputActionReference _walkToggleInput;
        [SerializeField] private InputActionReference _crouchInput;

        [Header("Events")]

        public UnityEvent<Vector2> onMovement = new UnityEvent<Vector2>();
        public UnityEvent<bool> onCrouch = new UnityEvent<bool>();

        [Header("Movement")]

        [SerializeField, Min(0)] private float _movementSpeedRun;
        [SerializeField, Min(0)] private float _movementSpeedWalk;
        private bool _running;
        private float _movementSpeedMax;
        private float _speedCurrent = 0;
        [SerializeField, Min(0)] private float _accelerationDuration;
        private float _acceleration;
        [SerializeField, Min(0)] private float _deccelerationDuration;
        private float _decceleration;
        private int _movementBlock;

        [Header("Rotation")]

        [SerializeField] private Transform _visualTransform;
        [SerializeField, Range(0f, 0.99f)] private float _turnSmoothFactor;

        [Header("Collider")]

        [SerializeField] private float _walkColliderHeight;

        [Header("Crouch")]

        [SerializeField, Min(0)] private float _crouchSpeedMax;
        public bool Crouching { get; private set; } = false;
        [SerializeField, Min(0)] private float _walkCameraHeight;
        [SerializeField, Min(0)] private float _crouchColliderHeight;
        [SerializeField, Min(0)] private float _crouchCameraHeight;
        [SerializeField] private LayerMask _terrain;

        [Space(24)]

        [SerializeField, Min(0)] private float _crouchHeightChangeDuration;
        private Coroutine _crouchRoutine;

        [Header("Hiding")]

        public HidingState hidingState;
        public enum HidingState {
            None,
            Wardrobe,
            Garbage,
            Bush
        }

        [Header("Camera")]

        [SerializeField] private Transform _cameraAimTargetRotator;
        [SerializeField] private Transform _overTheShoulderTarget;
        [SerializeField] private LayerMask _overTheShoulderSpringCollisions;

        [Header("Cache")]

        private Vector2 _inputCache = Vector3.zero;
        private Vector3 _directionCache;
        private Vector3 _movementCache;
        private Quaternion _targetAngle;

        private CharacterController _characterController;
        private Transform _cameraTransform;

        public Vector3 VisualForward { get { return _visualTransform.forward; } }

        protected override void Awake() {
            base.Awake();

            _movementInput.action.performed += MoveDirection;
            _movementInput.action.canceled += MoveDirection;
            _walkToggleInput.action.started += ToggleWalk;
            _crouchInput.action.started += Crouch;

            _acceleration = Time.fixedDeltaTime / _accelerationDuration;
            _decceleration = Time.fixedDeltaTime / _deccelerationDuration;
            _movementSpeedMax = _movementSpeedRun;
            _movementCache = Physics.gravity;

            _characterController = GetComponent<CharacterController>();
            _cameraTransform = Camera.main.transform; //

            Logger.Log(LogType.Player, $"{typeof(PlayerMovement).Name} Initialized");
        }

        private void Update() {
            if (_movementBlock <= 0) {
                Move();
                Rotate();
            }
        }

        private void MoveDirection(InputAction.CallbackContext input) {
            _inputCache = input.ReadValue<Vector2>();

            Logger.Log(LogType.Player, $"Movement Input Change: {input.ReadValue<Vector2>()}");
        }

        private void Move() {
            if (_inputCache.sqrMagnitude > 0) _targetAngle = Quaternion.Euler(0, Mathf.Atan2(_inputCache[0], _inputCache[1]) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y, 0);
            _speedCurrent = Mathf.Clamp(_speedCurrent + (_inputCache.sqrMagnitude > 0 ? _acceleration : -_decceleration), 0, _movementSpeedMax); // Could use _decceleration when above max speed
            _directionCache = (_targetAngle * Vector3.forward).normalized * _speedCurrent;
            _movementCache[0] = _directionCache[0];
            _movementCache[1] = _characterController.isGrounded ? (Physics.gravity.y / 10f) : Mathf.Clamp(_movementCache[1] + (Physics.gravity.y * Time.deltaTime), -50f, (Physics.gravity.y / 10f));
            _movementCache[2] = _directionCache[2];
            _characterController.Move(_movementCache * Time.deltaTime);

            onMovement?.Invoke(_speedCurrent * _inputCache);
        }

        private void SetColliderHeight(float height) {
            _characterController.height = height;
            _characterController.center = (height / 2f) * Vector3.up;
        }

        private void Crouch(InputAction.CallbackContext input) {
            if (!Physics.Raycast(transform.position, transform.up, _walkColliderHeight, _terrain)) {
                Crouching = !Crouching;
                _movementSpeedMax = Crouching ? _crouchSpeedMax : (_running ? _movementSpeedRun : _movementSpeedWalk);
                SetColliderHeight(Crouching ? _crouchColliderHeight : _walkColliderHeight);

                if (_crouchRoutine != null) StopCoroutine(_crouchRoutine);
                _crouchRoutine = StartCoroutine(CrouchSmoothHeightRoutine());

                onCrouch?.Invoke(Crouching);

                Logger.Log(LogType.Player, $"Crouch Toggle: {Crouching}");
            }
            else Logger.Log(LogType.Player, $"Crouch Toggle Fail: Terrain Above");
        }

        private IEnumerator CrouchSmoothHeightRoutine() {
            float duration = 0;
            float startHeight = _cameraAimTargetRotator.localPosition.y;
            float finalHeight = Crouching ? _crouchCameraHeight : _walkCameraHeight;
            while (duration < 1) {
                duration += Time.deltaTime / _crouchHeightChangeDuration;

                _cameraAimTargetRotator.localPosition = Mathf.Lerp(startHeight, finalHeight, duration) * Vector3.up;

                yield return null;
            }
        }

        private void Rotate() {
            _cameraAimTargetRotator.eulerAngles = _cameraTransform.eulerAngles.y * Vector3.up;
            _visualTransform.rotation = Quaternion.Slerp(_visualTransform.rotation, _targetAngle, _turnSmoothFactor);
        }

        private void ToggleWalk(InputAction.CallbackContext input) {
            _running = !_running;
            if(!Crouching) _movementSpeedMax = _running ? _movementSpeedRun : _movementSpeedWalk;
        }

        public void ToggleMovement(bool canMove) {
            _movementBlock += canMove ? -1 : 1;
            if (_movementBlock > 0) {
                _speedCurrent = 0f;
                onMovement?.Invoke(Vector2.zero);
            }
            Logger.Log(LogType.Player, $"Movement Blockers {(canMove ? "Increase" : "Decrease")} to: {_movementBlock}");
        }

        public void SetPosition(Vector3 position) {
            _characterController.enabled = false;
            transform.position = position;
            _characterController.enabled = true;
        }

        public void SetTargetAngle(float angle) {
            _targetAngle = Quaternion.Euler(0f, angle, 0f);
            _visualTransform.rotation = _targetAngle;
            // cinemachine freelook
        }

        public void UpdateVisualsVisibility(bool isVisible) {
            _visualTransform.gameObject.SetActive(isVisible);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            _characterController = GetComponent<CharacterController>();
            SetColliderHeight(_walkColliderHeight);
        }
#endif
    }
}