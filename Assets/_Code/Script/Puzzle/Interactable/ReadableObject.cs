using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.Dialogue;
using Ivayami.Audio;

namespace Ivayami.Puzzle {
    [RequireComponent(typeof(InteractableSounds))]
    public class ReadableObject : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableFeedbacks { get; private set; }
        private InteractableSounds _interactableSounds;

        [Header("Reading")]

        [SerializeField] private Readable _readable;
        [SerializeField] private bool _goesToInventory; // Rethink system

        [Header("Callbacks")]
        [SerializeField] private UnityEvent _onInteract;

        [Header("Animation")]

        private CameraAnimationInfo _focusCamera;

        private const string BLOCKER_KEY = "Readable";

        private void Awake() {
            InteratctableFeedbacks = GetComponent<InteractableFeedbacks>();
            _focusCamera = GetComponentInChildren<CameraAnimationInfo>();
            _interactableSounds = GetComponent<InteractableSounds>();

            if (!PlayerInventory.Instance) return;
            gameObject.SetActive(PlayerInventory.Instance.CheckInventoryFor(_readable.name).Item == null);
        }

        public PlayerActions.InteractAnimation Interact() {
            PlayerActions.Instance.ChangeInputMap("Menu");
            PlayerActions.Instance.ToggleInteract(nameof(ReadableObject), false);
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, false);
            InteratctableFeedbacks.UpdateFeedbacks(false, true);
            _focusCamera.StartMovement();

            ReadableUI.Instance.ShowReadable(_readable);
            ReturnAction.Instance.Set(StopReading);

            if (_goesToInventory) {
                PlayerInventory.Instance.AddToInventory(new ReadableItem(_readable.name));
                gameObject.SetActive(false);
            }

            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            _onInteract?.Invoke();
            return PlayerActions.InteractAnimation.Default;
        }

        public void StopReading() {
            PlayerActions.Instance.ChangeInputMap("Player");
            InteratctableFeedbacks.UpdateFeedbacks(true, true);
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, true);
            PlayerActions.Instance.ToggleInteract(nameof(ReadableObject), true);
            _focusCamera.ExitDialogueCamera();
            ReadableUI.Instance.Menu.Close();
        }

    }
}