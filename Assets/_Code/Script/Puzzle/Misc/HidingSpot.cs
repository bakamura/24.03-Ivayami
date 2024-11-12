using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;
using System.Collections;
using Ivayami.Dialogue;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle {
    public class HidingSpot : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableFeedbacks { get; private set; }
        [Header("Inputs")]
        [SerializeField] private InputActionReference _exitInput;

        [Header("View")]
        [SerializeField] private CameraAnimationInfo _hidingCam;
        [SerializeField] private CameraAnimationInfo _hiddenCam;

        [Header("Type")]

        [SerializeField] private Transform _animationPoint;
        [SerializeField] private PlayerMovement.HidingState _hiddenType;

        [Header("Cache")]

        private WaitForSeconds _delayChangeCamera;
        private Animator _objectAnimator;
        private Coroutine _hideCoroutine;
        private const string BLOCK_KEY = "hidingSpot";
        private bool _inputActive;

        private void Awake() {
            InteratctableFeedbacks = GetComponent<InteractableFeedbacks>();
            _objectAnimator = GetComponentInChildren<Animator>();
        }

        private void Start() {
            if (!PlayerActions.Instance || !PlayerAnimation.Instance) return;
            _delayChangeCamera = new WaitForSeconds(PlayerAnimation.Instance.GetInteractAnimationDuration(PlayerActions.InteractAnimation.EnterLocker) - _hidingCam.Duration);
        }

        public PlayerActions.InteractAnimation Interact() {
            if (PlayerMovement.Instance.hidingState == PlayerMovement.HidingState.None) {                
                _hideCoroutine = StartCoroutine(HideRoutine());
                return PlayerActions.InteractAnimation.EnterLocker;
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
            _hidingCam.StartMovement();
            PlayerMovement.Instance.SetPosition(_animationPoint.position);
            PlayerMovement.Instance.SetTargetAngle(_animationPoint.eulerAngles.y);
            InteratctableFeedbacks.UpdateFeedbacks(false, true);
            if (_objectAnimator) _objectAnimator.SetTrigger("Play");

            yield return _delayChangeCamera;

            _hidingCam.ExitDialogueCamera();
            _hiddenCam.StartMovement();
            PlayerMovement.Instance.hidingState = _hiddenType; 
            ReturnAction.Instance.Set(Exit);
            _exitInput.action.started += HandleExit;
            _exitInput.action.Enable();
            PlayerMovement.Instance.ToggleMovement(nameof(HidingSpot), false);
            _inputActive = true;
            _hideCoroutine = null;
        }        

        public void Exit() {
            if (_inputActive)
            {
                _exitInput.action.started -= HandleExit;
                PlayerMovement.Instance.ToggleMovement(nameof(HidingSpot), true);
            }
            _inputActive = false;
            PlayerStress.Instance.onFail.RemoveListener(OnPlayerDeath);
            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
            PlayerActions.Instance.ChangeInputMap("Player");
            Pause.Instance.ToggleCanPause(BLOCK_KEY, true);
            InteratctableFeedbacks.UpdateFeedbacks(true, true);
            PlayerAnimation.Instance.GoToIdle();

            _hiddenCam.ExitDialogueCamera();
        }

        private void OnPlayerDeath()
        {
            if (_inputActive)
            {
                _exitInput.action.started -= HandleExit;
                PlayerMovement.Instance.ToggleMovement(nameof(HidingSpot), true);
            }
            _inputActive = false;
            PlayerStress.Instance.onFail.RemoveListener(OnPlayerDeath);
            _objectAnimator.speed = 0;
            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
            PlayerActions.Instance.ChangeInputMap("Player");
            Pause.Instance.ToggleCanPause(BLOCK_KEY, true);
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
            _hiddenCam.ExitDialogueCamera();
            _hidingCam.ExitDialogueCamera();
        }

        private void HandleExit(InputAction.CallbackContext obj)
        {
            Exit();
        }
    }
}