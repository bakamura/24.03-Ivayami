using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class HidingSpot : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableHighlight { get; private set; }

        [Header("Input")]

        [SerializeField] private InputActionReference _exitInput;

        [Header("View")]

        [SerializeField] private CinemachineVirtualCamera _hiddenCam;
        private int _playerCamPriority;

        [Header("Type")]

        [SerializeField] private Transform _animationPoint;
        [SerializeField] private PlayerMovement.HidingState _hiddenType;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableFeedbacks>();
            _playerCamPriority = FindObjectOfType<CinemachineFreeLook>().Priority;
        }

        public void Interact() {
            if (PlayerMovement.Instance.hidingState == PlayerMovement.HidingState.None) {
                PlayerMovement.Instance.hidingState = _hiddenType;
                PlayerActions.Instance.ChangeInputMap("Menu");
                _exitInput.action.performed += Exit;

                _hiddenCam.Priority = _playerCamPriority + 1;
                PlayerMovement.Instance.SetPosition(_animationPoint.position);
                PlayerMovement.Instance.SetTargetAngle(_animationPoint.eulerAngles.y);
            }
            else {
                Debug.LogWarning($"Trying to interact with '{name}' while already hidden");
            }
        }

        public void Exit(InputAction.CallbackContext context) {
            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
                PlayerActions.Instance.ChangeInputMap("Player");
                _exitInput.action.performed -= Exit;

            _hiddenCam.Priority = _playerCamPriority - 1;
        }

    }
}