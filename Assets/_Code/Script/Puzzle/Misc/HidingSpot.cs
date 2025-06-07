using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;
using System.Collections;
using Ivayami.Dialogue;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Ivayami.Puzzle {
    public class HidingSpot : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableFeedbacks {
            get {
                if (!m_interactableFeedbacks) m_interactableFeedbacks = gameObject?.GetComponent<InteractableFeedbacks>();
                return m_interactableFeedbacks;
            }
        }
        private InteractableFeedbacks m_interactableFeedbacks;
        [Header("Inputs")]
        [SerializeField] private InputActionReference _exitInput;
        [SerializeField] private InputActionReference _lookSideways;

        [Header("View")]
        [SerializeField] private CameraAnimationInfo _hidingCam;
        [SerializeField] private CameraAnimationInfo _hiddenCamLeft;
        [SerializeField] private CameraAnimationInfo _hiddenCamMiddle;
        [SerializeField] private CameraAnimationInfo _hiddenCamRight;

        [Header("Type")]

        [SerializeField] private Transform _animationPoint;
        [SerializeField] private PlayerMovement.HidingState _hiddenType;
        [SerializeField] private PlayerActions.InteractAnimation _interactionType;

        [Header("Callbacks")]
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onExit;

        [Header("Cache")]

        private WaitForSeconds _delayChangeCamera;
        private Animator _objectAnimator;
        private Coroutine _hideCoroutine;
        private CameraAnimationInfo _currentCamera;
        private const string BLOCK_KEY = "hidingSpot";
        private static int SPEED = Animator.StringToHash("Speed");
        private bool _inputActive;

        private void Setup() {
            if (_delayChangeCamera == null) {
                _objectAnimator = GetComponentInChildren<Animator>();
                _delayChangeCamera = new WaitForSeconds(PlayerAnimation.Instance.GetInteractAnimationDuration(_interactionType) - _hidingCam.Duration);
                _objectAnimator.SetFloat(SPEED, PlayerAnimation.Instance.GetInteractAnimationSpeed(_interactionType));
            }
        }

        public PlayerActions.InteractAnimation Interact() {
            if (PlayerMovement.Instance.hidingState == PlayerMovement.HidingState.None) {
                Setup();
                _onInteract?.Invoke();
                _hideCoroutine = StartCoroutine(HideRoutine());
                return _interactionType;
            }
            else {
                Debug.LogWarning($"Trying to interact with '{name}' while already hidden");
                return PlayerActions.InteractAnimation.Default;
            }
        }

        private IEnumerator HideRoutine() {
            PlayerStress.Instance.onFail.AddListener(OnPlayerDeath);
            PlayerActions.Instance.ChangeInputMap("Menu");
            Pause.Instance.ToggleCanPause(BLOCK_KEY, false);
            PlayerAnimation.Instance.InteractLong(true);
            _hidingCam.StartMovement();
            _currentCamera = _hidingCam;
            PlayerMovement.Instance.SetPosition(_animationPoint.position);
            PlayerMovement.Instance.SetTargetAngle(_animationPoint.eulerAngles.y);
            InteratctableFeedbacks.UpdateFeedbacks(false, true);
            if (_objectAnimator) _objectAnimator.SetTrigger("Play");

            yield return _delayChangeCamera;

            _hidingCam.ExitDialogueCamera();
            _hiddenCamMiddle.StartMovement();
            _currentCamera = _hiddenCamMiddle;
            _inputActive = true;
            PlayerMovement.Instance.hidingState = _hiddenType;
            ReturnAction.Instance.Set(Exit);
            _exitInput.action.started += HandleExit;
            _lookSideways.action.performed += HandleLookSideways;
            _exitInput.action.Enable();
            PlayerMovement.Instance.ToggleMovement(nameof(HidingSpot), false);
            _hideCoroutine = null;
        }

        private void OnPlayerDeath() {
            StartCoroutine(RemovePlayer());
            _objectAnimator.speed = 0;
            if (_hideCoroutine != null) {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
            _hidingCam.ExitDialogueCamera();
        }

        private void HandleExit(InputAction.CallbackContext obj) {
            Exit();
        }

        private void Exit() {
            StartCoroutine(RemovePlayer());
            InteratctableFeedbacks.UpdateFeedbacks(true, true);
            PlayerAnimation.Instance.GoToIdle();
        }

        private void HandleLookSideways(InputAction.CallbackContext obj)
        {
            float input = obj.ReadValue<float>();
            if (input > 0 && _currentCamera != _hiddenCamRight)
            {
                _hiddenCamRight.StartMovement();
                _currentCamera = _hiddenCamRight;
            }
            else if (input < 0 && _currentCamera != _hiddenCamLeft)
            {
                _hiddenCamLeft.StartMovement();
                _currentCamera = _hiddenCamLeft;
            }
            else if (input == 0 && _currentCamera != _hiddenCamMiddle)
            {
                _hiddenCamMiddle.StartMovement();
                _currentCamera = _hiddenCamMiddle;
            }
        }

        private IEnumerator RemovePlayer() {
            if (_inputActive) {
                _exitInput.action.started -= HandleExit;
                _lookSideways.action.performed -= HandleLookSideways;
                PlayerMovement.Instance.ToggleMovement(nameof(HidingSpot), true);
            }
            _inputActive = false;
            PlayerStress.Instance.onFail.RemoveListener(OnPlayerDeath);
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerAnimation.Instance.InteractLong(false);
            Pause.Instance.ToggleCanPause(BLOCK_KEY, true);
            ReturnAction.Instance.Set(null);
            _currentCamera.ExitDialogueCamera();

            yield return null;

            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
            _onExit?.Invoke();
        }

    }
}