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

        [SerializeField] private float _movementSpeedRun;
        [SerializeField] private float _movementSpeedWalk;
        private float _movementSpeedMax;
        private float _speedCurrent = 0;
        [SerializeField] private float _accelerationDuration;
        private float _acceleration;
        [SerializeField] private float _deccelerationDuration;
        private float _decceleration;
        private bool _canMove;

        [Header("Rotation")]

        [SerializeField] private Transform _visualTransform;
        [SerializeField, Range(0f, 0.99f)] private float _turnSmoothFactor;

        [Header("Crouch")]

        [SerializeField] private float _crouchSpeedMax;
        public bool Crouching { get; private set; } = false;
        [SerializeField] private float _walkColliderHeight;
        [SerializeField] private float _walkCameraHeight;
        [SerializeField] private float _crouchColliderHeight;
        [SerializeField] private float _crouchCameraHeight;
        [SerializeField] private LayerMask _terrain;

        [Space(24)]

        [SerializeField] private float _crouchHeightChangeDuration;
        private Coroutine _crouchRoutine;

        [Header("Hiding")]

        public HidingState hidingState;
        public enum HidingState {
            None,
            Wardrobe,
            Garbage
        }

        [Header("Camera")]

        [SerializeField] private Transform _cameraAimTargetRotator;
        [SerializeField] private Transform _overTheShoulderTarget;
        private float _overTheShoulderMaxDistance;
        [SerializeField] private LayerMask _overTheShoulderSpringCollisions;

        [Header("Cache")]

        private Vector2 _inputCache = Vector3.zero;
        private Vector3 _movementDirectionCache = Vector3.zero;
        private Vector3 _velocityCache;
        private float _movementSpeedMaxCurrent;
        private Quaternion _targetAngle;
        //private float _directionDifferenceToInputAngleCache;

        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private Transform _cameraTransform;

        protected override void Awake() {
            base.Awake();

            _movementInput.action.performed += MoveDirection;
            _movementInput.action.canceled += MoveDirection;
            _walkToggleInput.action.started += ToggleWalk;
            _crouchInput.action.started += Crouch;

            _acceleration = Time.fixedDeltaTime / _accelerationDuration;
            _decceleration = Time.fixedDeltaTime / _deccelerationDuration;
            _movementSpeedMax = _movementSpeedRun;

            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _cameraTransform = Camera.main.transform; //
            _overTheShoulderMaxDistance = _overTheShoulderTarget.localPosition.x;

            Logger.Log(LogType.Player, $"{typeof(PlayerMovement).Name} Initialized");
        }

        private void Update() {
            if (_canMove) Rotate();
            //OverTheShoulderSpring();
        }

        private void FixedUpdate() {
            if (_canMove) Move();
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
                _movementSpeedMaxCurrent = _movementDirectionCache.magnitude * (Crouching ? _crouchSpeedMax : _movementSpeedMax);
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

        private void OverTheShoulderSpring() {
            _overTheShoulderTarget.localPosition = Vector3.right *
               (Physics.Raycast(_overTheShoulderTarget.position, _overTheShoulderTarget.right, out RaycastHit hit, _overTheShoulderMaxDistance, _overTheShoulderSpringCollisions) ?
                hit.distance : _overTheShoulderMaxDistance);
        }

        private void ToggleWalk(InputAction.CallbackContext input) {
            _movementSpeedMax = _movementSpeedMax != _movementSpeedRun ? _movementSpeedRun : _movementSpeedWalk;
        }

        public void ToggleMovement(bool canMove) {
            _canMove = canMove;
            if (!_canMove) _speedCurrent = 0f;
            _rigidbody.constraints = canMove ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeAll;

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

        public void SetPosition(Vector3 position) {
            _rigidbody.position = position;
        }

        public void SetTargetAngle(float angle){
            _targetAngle = Quaternion.Euler(0f, angle, 0f);
            _visualTransform.rotation = _targetAngle;
            // cinemachine freelook
        }

    }
}