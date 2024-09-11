using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.Dialogue;
using Ivayami.Save;
using Ivayami.Audio;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableSounds))]
    public class ReadableObject : MonoBehaviour, IInteractable
    {

        public InteractableFeedbacks InteratctableHighlight { get; private set; }
        private InteractableSounds _interactableSounds;

        [Header("Reading")]

        [SerializeField] private Readable _readable;
        [SerializeField] private bool _goesToInventory;

        [Header("Animation")]

        private CameraAnimationInfo _focusCamera;

        private void Awake()
        {
            InteratctableHighlight = GetComponent<InteractableFeedbacks>();
            _focusCamera = GetComponentInChildren<CameraAnimationInfo>();
            _interactableSounds = GetComponent<InteractableSounds>();
        }

        public PlayerActions.InteractAnimation Interact()
        {
            PlayerActions.Instance.ChangeInputMap("Menu");
            Pause.Instance.canPause = false;
            _focusCamera.StartMovement();

            Readable readable = _readable.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language);
            ReadableUI.Instance.ShowReadable(readable.Title, readable.Content);

            ReadableUI.Instance.CloseBtn.onClick.AddListener(StopReading);
            ReturnAction.Instance.Set(StopReading);

            if (_goesToInventory)
            {
                // TO DO
            }

            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            return PlayerActions.InteractAnimation.Default;
        }

        public void StopReading()
        {
            PlayerActions.Instance.ChangeInputMap("Player");
            Pause.Instance.canPause = true;
            _focusCamera.ExitDialogueCamera();
            ReadableUI.Instance.Menu.Close();
            ReadableUI.Instance.CloseBtn.onClick.RemoveAllListeners();
        }

    }
}