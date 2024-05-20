using UnityEngine;
using Cinemachine;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class ReadableObject : MonoBehaviour, IInteractable {

        public InteractableHighlight InteratctableHighlight { get; private set; }

        [Header("Reading")]

        [SerializeField] private Readable _readable;
        [SerializeField] private bool _goesToInventory;

        [Header("Animation")]

        [SerializeField] private CinemachineVirtualCamera _focusCamera;
        private int _focusedCameraPriority;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableHighlight>();
        }

        private void Start() {
            _focusedCameraPriority = FindObjectOfType<CinemachineFreeLook>().Priority + 1;
        }

        public void Interact() {
            _focusCamera.Priority = _focusedCameraPriority;
            PlayerActions.Instance.ChangeInputMap("Menu");
            ReadableUI.Instance.ShowReadable(_readable.name, _readable.Content);
            ReadableUI.Instance.CloseBtn.onClick.AddListener(StopReading);
            if (_goesToInventory) { }
        }

        public void StopReading() {
            _focusCamera.Priority = _focusedCameraPriority - 2;
            PlayerActions.Instance.ChangeInputMap("Player");
            ReadableUI.Instance.Menu.Close();
            ReadableUI.Instance.CloseBtn.onClick.RemoveAllListeners();
        }

    }
}