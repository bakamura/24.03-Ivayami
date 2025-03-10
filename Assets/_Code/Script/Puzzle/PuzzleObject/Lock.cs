using UnityEngine;
using Ivayami.Player;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Ivayami.Audio;
using System.Collections;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds), typeof(LockPuzzleSounds))]
    public class Lock : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionReference _confirmInput;
        [SerializeField] private InputActionReference _clickInput;

        [SerializeField, Min(0f)] private float _unlockDelay;
        [SerializeField] private InteractionTypes _interactionType;

        [SerializeField] private PasswordUI _passwordUI;
        [SerializeField] private DeliverUI _deliveryUI;

        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;
        [SerializeField] private UnityEvent _onInteractionFailed;

        private InteractableFeedbacks _interatctableFeedbacks;
        private InteractableSounds _interactableSounds;
        private LockPuzzleSounds _lockSounds;
        private WaitForSeconds _unlockWait;
        private Coroutine _unlockCoroutine;
        public InteractableFeedbacks InteratctableFeedbacks { get => _interatctableFeedbacks; }
        public LockPuzzleSounds LockSounds => _lockSounds;

        [System.Serializable]
        public enum InteractionTypes
        {
            RequireItems,
            RequirePassword
        }

        [System.Serializable]
        private struct ItemRequestData
        {
            public InventoryItem Item;
            public bool UseItem;
            public UnityEvent OnItemDelivered;
        }

        private void Awake()
        {
            _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _lockSounds = GetComponent<LockPuzzleSounds>();
            _unlockWait = new WaitForSeconds(_unlockDelay);
        }

        public void ForceInteract() => Interact();

        [ContextMenu("Interact")]
        public PlayerActions.InteractAnimation Interact()
        {
            _onInteract?.Invoke();
            //if (_interactionType == InteractionTypes.RequireItems)
            //{
            //    _deliveryUI.Open();
            //    return PlayerActions.InteractAnimation.Default;
            //}
            UpdateInputs(true);
            _interatctableFeedbacks.UpdateFeedbacks(false, true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            UpdateUIs(true);
            return PlayerActions.InteractAnimation.Default;
        }

        public void TryUnlock()
        {
            if (_unlockCoroutine == null)
            {
                bool isPasswordCorrect = _interactionType == InteractionTypes.RequirePassword && _passwordUI.CheckPassword();
                bool hasDeliveredAllItems = _interactionType == InteractionTypes.RequireItems && _deliveryUI.CheckRequestsCompletion();
                if (hasDeliveredAllItems || isPasswordCorrect)
                {
                    _unlockCoroutine = StartCoroutine(UnlockFeedbackCoroutine());
                }
                else
                {
                    _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionFailed);
                    if (_deliveryUI.SkipDeliverUI)
                    {
                        _interatctableFeedbacks.UpdateFeedbacks(true, true);
                        UpdateUIs(false);
                        UpdateInputs(false);
                    }
                    _onInteractionFailed?.Invoke();
                }
            }
        }

        private IEnumerator UnlockFeedbackCoroutine()
        {
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionSuccess);
            yield return _unlockWait;
            UpdateUIs(false);
            UpdateInputs(false);
            IsActive = !IsActive;
            onActivate?.Invoke();
            _unlockCoroutine = null;
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _cancelInteractionInput.action.started += HandleExitInteraction;
                if (_interactionType == InteractionTypes.RequirePassword)
                {
                    _passwordUI.OnCheckPassword += TryUnlock;
                    if (_passwordUI is RotateLock)
                    {
                        _confirmInput.action.started += HandleConfirmUI;
                        _clickInput.action.performed += HandleConfirmUI;
                    }
                }
                PlayerMovement.Instance.ToggleMovement(nameof(Lock), false);
                PlayerActions.Instance.ChangeInputMap("Menu");
                PlayerActions.Instance.ToggleInteract(nameof(Lock), false);
            }
            else
            {
                _cancelInteractionInput.action.started -= HandleExitInteraction;
                if (_interactionType == InteractionTypes.RequirePassword)
                {
                    _passwordUI.OnCheckPassword -= TryUnlock;
                    if (_passwordUI is RotateLock)
                    {
                        _confirmInput.action.started -= HandleConfirmUI;
                        _clickInput.action.performed -= HandleConfirmUI;
                    }
                }
                PlayerMovement.Instance.ToggleMovement(nameof(Lock), true);
                PlayerActions.Instance.ChangeInputMap("Player");
                PlayerActions.Instance.ToggleInteract(nameof(Lock), true);
            }
        }
        private void UpdateUIs(bool isActive)
        {
            if (_interactionType == InteractionTypes.RequirePassword) _passwordUI.UpdateActiveState(isActive);
            else _deliveryUI.UpdateUI(isActive);
        }

        private void CancelInteraction()
        {
            UpdateUIs(false);
            UpdateInputs(false);
            _interatctableFeedbacks.UpdateFeedbacks(true, true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.InteractReturn);
            _onCancelInteraction?.Invoke();
        }

        private void HandleExitInteraction(InputAction.CallbackContext context)
        {
            CancelInteraction();
        }

        private void HandleConfirmUI(InputAction.CallbackContext context)
        {
            TryUnlock();
        }
    }
}