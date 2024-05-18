using UnityEngine;
using Cinemachine;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class ReadableObject : MonoBehaviour, IInteractable {

        public InteractableHighlight InteratctableHighlight { get; private set; }

        [SerializeField] private Readable _readable;
        [SerializeField] private bool _goesToInventory;

        [SerializeField] private ReadableUI _canvasReadable;
        [SerializeField] private CinemachineVirtualCamera _focusCamera;
        [SerializeField] private int _focusedCameraPriority;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableHighlight>();
        }

        private void Start() {
            _focusedCameraPriority = FindObjectOfType<CinemachineFreeLook>().Priority + 1;
        }

        public void Interact() {
            _focusCamera.Priority = _focusedCameraPriority;
            PlayerActions.Instance.ChangeInputMap("Menu");
            _canvasReadable.ShowReadable(_readable.name, _readable.Content);
            if (_goesToInventory) { }
        }

        public void StopReading() {
            _focusCamera.Priority = _focusedCameraPriority;
            PlayerActions.Instance.ChangeInputMap("Player");
            _canvasReadable.Menu.Close();
        }

    }
}