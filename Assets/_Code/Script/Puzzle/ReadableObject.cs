using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.Dialogue;

namespace Ivayami.Puzzle {
    public class ReadableObject : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableHighlight { get; private set; }

        [Header("Reading")]

        [SerializeField] private Readable _readable;
        [SerializeField] private bool _goesToInventory;

        [Header("Animation")]

        private CameraAnimationInfo _focusCamera;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableFeedbacks>();
            _focusCamera = GetComponentInChildren<CameraAnimationInfo>();
        }

        public void Interact() {
            PlayerActions.Instance.ChangeInputMap("Menu");
            PlayerActions.Instance.AllowPausing(false);
            DialogueCamera.Instance.MoveRotate(_focusCamera);
            ReadableUI.Instance.ShowReadable(_readable.name, _readable.Content);
            ReadableUI.Instance.CloseBtn.onClick.AddListener(StopReading);
            ReturnAction.Instance.Set(StopReading);
            if (_goesToInventory) { }
        }

        public void StopReading() {
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerActions.Instance.AllowPausing(true);
            DialogueCamera.Instance.ExitDialogeCamera();
            ReadableUI.Instance.Menu.Close();
            ReadableUI.Instance.CloseBtn.onClick.RemoveAllListeners();
        }

    }
}