using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Scene;
using Ivayami.Save;

namespace Ivayami.Player {
    public class PlayerMovement : MonoSingleton<PlayerMovement> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _movementInput;
        [SerializeField] private InputActionReference _walkToggleInput;
        [SerializeField] private InputActionReference _crouchInput;

        [Header("Events")]

        public UnityEvent<Vector2> onMovement = new UnityEvent<Vector2>();
        public UnityEvent<bool> onCrouch = new UnityEvent<bool>();
        public UnityEvent<float> onStaminaUpdate = new UnityEvent<float>();

        [Header("Movement")]

        [SerializeField, Min(0)] private float _movementSpeedRun;
        [SerializeField, Min(0)] private float _movementSpeedWalk;
        private bool _running = true;
        private float _movementSpeedMax;
        private float _speedCurrent = 0;
        [SerializeField, Min(0)] private float _accelerationDuration;
        private float _acceleration;
        [SerializeField, Min(0)] private float _deccelerationDuration;
        private float _decceleration;
        private HashSet<string> _movementBlock = new HashSet<string>();

        [Header("Stamina")]

        [SerializeField, Min(0)] private float _maxStamina = 100f;
        [SerializeField, Range(0,1)] private float _minStaminaToRun;
        [SerializeField, Range(0,1), Tooltip("Depletion per second")] private float _staminaDepletionRate = .1f;
        [SerializeField, Range(0, 1), Tooltip("Depletion per second")] private float _staminaRegenerationRate = .1f;
        [SerializeField, Range(0,1), Tooltip("When stress is greater or equal to this value, stamia depletion will start")] private float _staminaDepletionStressThreshold = .6f;
        //[SerializeField, Range(0,1)] private float _staminaFeedbackThreshold;
        private float _staminaCurrent;
        private float _stressCurrent;
        private float _maxStressCurrent;
        public bool CanMove {  get { return _movementBlock.Count <= 0; } }
        private bool _canRun = true;
        private bool _holdToRun;

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

        [HideInInspector] public HidingState hidingState;
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
        private SkinnedMeshRenderer[] _visualComponents;
        private byte _gravityFactor = 1;
        private float _stickDeadzone = .125f;

        private const string INTERACT_BLOCK_KEY = "Interact";

        public Vector3 VisualForward { get { return _visualTransform.forward; } }
#if UNITY_EDITOR
        public float MaxStamina => _maxStamina;
#endif

        protected override void Awake() 
            {
            base.Awake();

            _movementInput.action.performed += MoveDirection;
            _movementInput.action.canceled += MoveDirection;
            _walkToggleInput.action.started += ToggleWalkInput;
            _crouchInput.action.started += Crouch;

            _acceleration = Time.fixedDeltaTime / _accelerationDuration;
            _decceleration = Time.fixedDeltaTime / _deccelerationDuration;
            _movementSpeedMax = _movementSpeedRun;
            _movementCache = Physics.gravity;
            ResetStamina();           

            _characterController = GetComponent<CharacterController>();
            _visualComponents = _visualTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
            _cameraTransform = Camera.main.transform; //

            Logger.Log(LogType.Player, $"{typeof(PlayerMovement).Name} Initialized");
        }

        private void Start() {
            SceneController.Instance.OnAllSceneRequestEnd += RemoveCrouch;
            PlayerActions.Instance.onInteract.AddListener((animation) => BlockMovementFor(INTERACT_BLOCK_KEY, PlayerAnimation.Instance.GetInteractAnimationDuration(animation)));
            PlayerStress.Instance.onStressChange.AddListener(OnStressChange);
            InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateHoldToRun);
            _maxStressCurrent = PlayerStress.Instance.MaxStress;
        }        

        private void Update() {
            if (CanMove) Move();
            Rotate();
            StaminaUpdate();
        }

        private void MoveDirection(InputAction.CallbackContext input) {
            Vector2 value = input.ReadValue<Vector2>();
            if(value.magnitude > _stickDeadzone) _inputCache = value;
            else _inputCache = Vector2.zero;

            Logger.Log(LogType.Player, $"Movement Input Change: {input.ReadValue<Vector2>()}");
        }

        private void Move() {
            if (_inputCache.sqrMagnitude > 0) _targetAngle = Quaternion.Euler(0, Mathf.Atan2(_inputCache[0], _inputCache[1]) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y, 0);
            _speedCurrent = Mathf.Clamp(_speedCurrent + (_inputCache.sqrMagnitude > 0 ? _acceleration : -_decceleration), 0, _movementSpeedMax); // Could use _decceleration when above max speed
            _directionCache = (_targetAngle * Vector3.forward).normalized * _speedCurrent;
            _movementCache[0] = _directionCache[0];
            _movementCache[1] = _characterController.isGrounded ? Physics.gravity.y / 10f *_gravityFactor : Mathf.Clamp(_movementCache[1] + (Physics.gravity.y * Time.deltaTime), -50f, Physics.gravity.y / 10f) * _gravityFactor;
            _movementCache[2] = _directionCache[2];
            _characterController.Move(_movementCache * Time.deltaTime);

            onMovement?.Invoke(_speedCurrent * _inputCache);
        }

        private void SetColliderHeight(float height) {
            _characterController.height = height;
            _characterController.center = (height / 2f) * Vector3.up;
        }

        private void Crouch(InputAction.CallbackContext input) {
            if (CanMove) {
                if (!Physics.Raycast(transform.position, transform.up, _walkColliderHeight, _terrain)) ToggleCrouch();
                else Logger.Log(LogType.Player, $"Crouch Toggle Fail: Terrain Above");
            }
        }

        private void ToggleCrouch() {
            Crouching = !Crouching;
            _movementSpeedMax = Crouching ? _crouchSpeedMax : (_running ? _movementSpeedRun : _movementSpeedWalk);
            SetColliderHeight(Crouching ? _crouchColliderHeight : _walkColliderHeight);

            if (_crouchRoutine != null) StopCoroutine(_crouchRoutine);
            _crouchRoutine = StartCoroutine(CrouchSmoothHeightRoutine());

            onCrouch?.Invoke(Crouching);

            Logger.Log(LogType.Player, $"Crouch Toggle: {Crouching}");

        }

        private void RemoveCrouch() {
            if (Crouching) ToggleCrouch();
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

        private void ToggleWalkInput(InputAction.CallbackContext input = new InputAction.CallbackContext()) {
            if(_staminaCurrent > _maxStamina * _minStaminaToRun)ToggleWalk();
        }

        private void ToggleWalk()
        {
            if (_canRun)
            {
                _running = !_running;
                if (!Crouching) _movementSpeedMax = _running ? _movementSpeedRun : _movementSpeedWalk;
            }
        }

        private void OnStressChange(float currentStress)
        {
            _stressCurrent = currentStress;
        }

        private void StaminaUpdate()
        {
            bool inStressRange = _stressCurrent >= _staminaDepletionStressThreshold * _maxStressCurrent;
            if (!_running || _speedCurrent == 0)
            {
                if (inStressRange && _staminaCurrent < _maxStamina) UpdateCurrentStamina(_staminaRegenerationRate);                
            }
            else
            {
                if (inStressRange && _staminaCurrent > 0) UpdateCurrentStamina(-_staminaDepletionRate);
            }
            if (!inStressRange) ResetStamina();
        }

        private void UpdateCurrentStamina(float value)
        {
            _staminaCurrent = Mathf.Clamp(_staminaCurrent + value * _maxStamina * Time.deltaTime, 0, _maxStamina);
            if (_staminaCurrent <= 0) AllowRun(false);
            else AllowRun(true);
            onStaminaUpdate?.Invoke(_staminaCurrent / _maxStamina);
        }

        private void ResetStamina()
        {
            _staminaCurrent = _maxStamina;
            onStaminaUpdate?.Invoke(1);
        }

        public void AllowRun(bool allow) {
            if (!allow && _running) ToggleWalk();
            _canRun = allow;
        }

        public void ToggleMovement(string key, bool canMove) {
            if (canMove) {
                if (!_movementBlock.Remove(key)) Debug.LogWarning($"'{key}' tried to unlock movement but key isn't blocking");
            }
            else if (!_movementBlock.Add(key)) Debug.LogWarning($"'{key}' tried to lock movement but key is already blocking");

            if (_movementBlock.Count > 0) {
                _speedCurrent = 0f;
                onMovement?.Invoke(Vector2.zero);
            }
            Logger.Log(LogType.Player, $"Movement Blockers {(canMove ? "Increase" : "Decrease")} to: {_movementBlock.Count}");
        }

        public void BlockMovementFor(string key, float seconds) {
            StartCoroutine(BlockMovementRoutine(key, seconds));
        }

        private IEnumerator BlockMovementRoutine(string key, float seconds) {
            ToggleMovement(key, false);

            yield return new WaitForSeconds(seconds);

            ToggleMovement(key, true);
        }

        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        public void SetTargetAngle(float angle, bool isIstant = true) {
            _targetAngle = Quaternion.Euler(0f, angle, 0f);
            if (isIstant) _visualTransform.rotation = _targetAngle;
        }

        public void UpdateVisualsVisibility(bool isVisible) {
            for (int i = 0; i < _visualComponents.Length; i++)
                _visualComponents[i].enabled = isVisible;
            //_visualTransform.gameObject.SetActive(isVisible);
        }

        public void ChangeRunSpeed(float val)
        {
            if (!IngameDebugConsole.DebugLogManager.Instance) return;
            _movementSpeedRun = val;
            _movementSpeedMax = _movementSpeedRun;
        }

        public void RemoveAllBlockers() {
            if (!IngameDebugConsole.DebugLogManager.Instance) return;
            _movementBlock.Clear();
        }

        public void UpdatePlayerGravity(bool isActive)
        {
            _gravityFactor = (byte)(isActive ? 1 : 0);
        }

        public void ChangeStickDeadzone(float value)
        {
            _stickDeadzone = Mathf.Clamp(value, 0.1f, .5f);
        }

        public void ChangeHoldToRun(bool isActive)
        {
            _holdToRun = isActive;
            if (isActive)
            {
                _walkToggleInput.action.canceled += ToggleWalkInput;
                if (_running) ToggleWalk();
            }
            else _walkToggleInput.action.canceled -= ToggleWalkInput;
        }

        private void UpdateHoldToRun(bool isGamepad)
        {
            if (!SaveSystem.Instance || SaveSystem.Instance.Options == null) return;
            if (isGamepad && SaveSystem.Instance.Options.holdToRun && _holdToRun)
            {
                ChangeHoldToRun(false);
            }
            else if(!isGamepad && SaveSystem.Instance.Options.holdToRun && !_holdToRun)
            {
                ChangeHoldToRun(true);
            }
        }

#if UNITY_EDITOR

        private void OnValidate() {
            if(!_characterController)_characterController = GetComponent<CharacterController>();
            SetColliderHeight(_walkColliderHeight);
        }
#endif
    }
}