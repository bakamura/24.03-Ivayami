using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.Dialogue;
using Ivayami.Save;
using Ivayami.Audio;
using UnityEngine.Events;

namespace Ivayami.Puzzle {
    [RequireComponent(typeof(InteractableSounds))]
    public class ReadableObject : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableFeedbacks { get; private set; }
        private InteractableSounds _interactableSounds;

        [Header("Reading")]

        [SerializeField] private Readable _readable;
        [SerializeField] private bool _goesToInventory;

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
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, false);
            InteratctableFeedbacks.UpdateFeedbacks(false, true);
            _focusCamera.StartMovement();

            Readable readable = _readable.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language);
            ReadableUI.Instance.ShowReadable(readable.Title, readable.Content);

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
            _focusCamera.ExitDialogueCamera();
            ReadableUI.Instance.Menu.Close();
        }

    }
}