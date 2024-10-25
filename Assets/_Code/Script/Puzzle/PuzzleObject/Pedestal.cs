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
        private ActivatorAnimated _activatorAnimated;
        private InteractableFeedbacks _interactableFeedbacks;
        private InteractableSounds _interactableSounds;
        private InventoryItem _currentItem;
        private GameObject _currentItemVisual;

        public InteractableFeedbacks InteratctableFeedbacks => _interactableFeedbacks;
        public DeliverUI DeliverUI => _deliverUI;
        public ActivatorAnimated ActivatorAnimation => _activatorAnimated;
        private void Awake()
        {
            _interactableFeedbacks = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _activatorAnimated = GetComponent<ActivatorAnimated>();
            _deliverUI.OnDeliver.AddListener(HandleItemDeliver);
            if (_currentItem)
            {
                _currentItemVisual = Instantiate(_itemToStart.Model, _itemVisualPosition);
                IsActive = true;
                onActivate?.Invoke();
            }
        }

        public PlayerActions.InteractAnimation Interact()
        {
            if (_currentItem)
            {
                PlayerInventory.Instance.AddToInventory(_currentItem);
                Destroy(_currentItemVisual);
                _deliverUI.RevertItemDeliver(_currentItem);
                IsActive = false;
                onActivate?.Invoke();
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
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.InteractReturn);            
        }

        private void ExitInteraction()
        {
            _cancelInteractionInput.action.performed -= HandleExitInteraction;
            _deliverUI.UpdateUI(false);
            _interactableFeedbacks.UpdateFeedbacks(true, true);
            _onCancelInteraction?.Invoke();
        }

        private void HandleItemDeliver(InventoryItem item)
        {
            if (_currentItemVisual) Destroy(_currentItemVisual);
            _currentItemVisual = Instantiate(item.Model, _itemVisualPosition);
            _currentItem = item;
            IsActive = true;
            ExitInteraction();
            onActivate?.Invoke();
        }
    }
}