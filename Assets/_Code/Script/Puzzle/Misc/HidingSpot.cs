using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;
using System.Collections;
using Ivayami.Dialogue;

namespace Ivayami.Puzzle {
    public class HidingSpot : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableFeedbacks { get; private set; }

        [Header("View")]
        [SerializeField] private CameraAnimationInfo _hidingCam;
        [SerializeField] private CameraAnimationInfo _hiddenCam;

        [Header("Type")]

        [SerializeField] private Transform _animationPoint;
        [SerializeField] private PlayerMovement.HidingState _hiddenType;

        [Header("Cache")]

        private WaitForSeconds _delayChangeCamera;
        private Animator _objectAnimator;

        private void Awake() {
            InteratctableFeedbacks = GetComponent<InteractableFeedbacks>();
            _objectAnimator = GetComponent<Animator>();
        }

        private void Start() {
            if (!PlayerActions.Instance || !PlayerAnimation.Instance) return;
            _delayChangeCamera = new WaitForSeconds(PlayerAnimation.Instance.GetInteractAnimationDuration(PlayerActions.InteractAnimation.EnterLocker) - _hidingCam.Duration);
        }

        public PlayerActions.InteractAnimation Interact() {
            if (PlayerMovement.Instance.hidingState == PlayerMovement.HidingState.None) {
                StartCoroutine(HideRoutine());
                return PlayerActions.InteractAnimation.EnterLocker;
            }
            else {
                Debug.LogWarning($"Trying to interact with '{name}' while already hidden");
                return PlayerActions.InteractAnimation.Default;
            }
        }

        private IEnumerator HideRoutine() {
            PlayerActions.Instance.ChangeInputMap("Menu");
            _hidingCam.StartMovement();
            PlayerMovement.Instance.SetPosition(_animationPoint.position);
            PlayerMovement.Instance.SetTargetAngle(_animationPoint.eulerAngles.y);
            if (_objectAnimator) _objectAnimator.SetTrigger("Play");

            yield return _delayChangeCamera;

            _hidingCam.ExitDialogueCamera();
            _hiddenCam.StartMovement();
            PlayerMovement.Instance.hidingState = _hiddenType; 
            ReturnAction.Instance.Set(Exit);
        }

        public void Exit() {
            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerAnimation.Instance.GoToIdle();

            _hiddenCam.ExitDialogueCamera();
        }

    }
}