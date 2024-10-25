using Ivayami.Audio;
using Ivayami.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated), typeof(InteractableSounds))]
    public class Pedestal : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private DeliverUI _deliverUI;
        [SerializeField] private Transform _itemVisualPosition;
        [SerializeField] private InventoryItem _itemToStart;
        [SerializeField] private UnityEvent _onDeliverUIOpen;
        [SerializeField] private UnityEvent _onCancelInteraction;
        public InteractableFeedbacks InteratctableFeedbacks => _interactableFeedbacks;
        private InteractableFeedbacks _interactableFeedbacks;
        private ActivatorAnimated _activatorAnimated;
        private InteractableSounds _interactableSounds;
        private InventoryItem _currentItem;
        private GameObject _currentItemVisual;

        private void Awake()
        {
            _interactableFeedbacks = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _activatorAnimated = GetComponent<ActivatorAnimated>();
            _deliverUI.OnDeliver.AddListener(HandleItemDeliver);
            if (_currentItem)
            {
                _currentItemVisual = Instantiate(_itemToStart.Model, _itemVisualPosition);
                _activatorAnimated.Activate(true);
            }
        }

        public PlayerActions.InteractAnimation Interact()
        {
            if (_currentItem)
            {
                PlayerInventory.Instance.AddToInventory(_currentItem);
                Destroy(_currentItemVisual);
                _deliverUI.RevertItemDeliver(_currentItem);
                _activatorAnimated.Activate(false);
                _currentItem = null;
            }
            else
            {
                _cancelInteractionInput.action.performed += HandleExitInteraction;
                _deliverUI.UpdateUI(true);
                _onDeliverUIOpen?.Invoke();
            }
            return PlayerActions.InteractAnimation.Default;
        }

        private void HandleExitInteraction(InputAction.CallbackContext obj)
        {
            ExitInteraction();            
        }

        private void ExitInteraction()
        {
            _cancelInteractionInput.action.performed -= HandleExitInteraction;
            _deliverUI.UpdateUI(false);
            _interactableFeedbacks.UpdateFeedbacks(true, true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.InteractReturn);
            _onCancelInteraction?.Invoke();
        }

        private void HandleItemDeliver(InventoryItem item)
        {
            if (_currentItemVisual) Destroy(_currentItemVisual);
            _currentItemVisual = Instantiate(item.Model, _itemVisualPosition);
            _activatorAnimated.Activate(true);
            _currentItem = item;
            IsActive = _deliverUI.CheckRequestsCompletion();
            ExitInteraction();
            onActivate?.Invoke();
        }
    }
}