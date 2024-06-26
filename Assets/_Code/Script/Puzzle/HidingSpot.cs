using UnityEngine;
using Cinemachine;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class HidingSpot : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableHighlight { get; private set; }

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
                _hiddenCam.Priority = _playerCamPriority + 1;
                PlayerMovement.Instance.SetPosition(_animationPoint.position);
                PlayerMovement.Instance.SetTargetAngle(_animationPoint.eulerAngles.y);
            }
            else {
                PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
                _hiddenCam.Priority = _playerCamPriority - 1;
            }
        }

    }
}