using UnityEngine;
using Cinemachine;
using Ivayami.Player;
using Ivayami.UI;
using System.Collections;

namespace Ivayami.Puzzle {
    public class HidingSpot : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableHighlight { get; private set; }

        [Header("View")]

        [SerializeField] private CinemachineVirtualCamera _hidingCam;
        [SerializeField] private CinemachineVirtualCamera _hiddenCam;
        private int _playerCamPriority;

        [Header("Type")]

        [SerializeField] private Transform _animationPoint;
        [SerializeField] private PlayerMovement.HidingState _hiddenType;

        [Header("Cache")]

        private WaitForSeconds _delayChangeCamera;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableFeedbacks>();
            _playerCamPriority = FindObjectOfType<CinemachineFreeLook>().Priority;
        }

        private void Start() {
            _delayChangeCamera = new WaitForSeconds(PlayerAnimation.Instance.GetInteractAnimationDuration(PlayerActions.InteractAnimation.EnterLocker) - Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.BlendTime);
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
            _hidingCam.Priority = _playerCamPriority + 1;
            PlayerMovement.Instance.SetPosition(_animationPoint.position);
            PlayerMovement.Instance.SetTargetAngle(_animationPoint.eulerAngles.y);

            yield return _delayChangeCamera;

            _hidingCam.Priority = _playerCamPriority - 1;
            _hiddenCam.Priority = _playerCamPriority + 1;
            PlayerMovement.Instance.hidingState = _hiddenType; 
            ReturnAction.Instance.Set(Exit);
        }

        public void Exit() {
            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerAnimation.Instance.GoToIdle();

            _hiddenCam.Priority = _playerCamPriority - 1;
        }

    }
}