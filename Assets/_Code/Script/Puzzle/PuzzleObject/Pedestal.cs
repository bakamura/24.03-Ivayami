using Ivayami.Audio;
using Ivayami.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
    public class Pedestal : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private DeliverUI _deliverUI;
        [SerializeField] private Transform _itemVisualPosition;
        [SerializeField] private InventoryItem _itemToStart;
        [SerializeField] private UnityEvent _onItemCollected;
        [SerializeField] private UnityEvent _onDeliverUIOpen;
        [SerializeField] private UnityEvent _onExitInteraction;

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

        public void ForceInteract() => Interact();

        public PlayerActions.InteractAnimation Interact()
        {
            if (_currentItem)
            {
                PlayerInventory.Instance.AddToInventory(_currentItem);
                Destroy(_currentItemVisual);
                _deliverUI.RevertItemDeliver(_currentItem);
                DeactivatePedestal();
                _currentItem = null;
                _onItemCollected?.Invoke();
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
            _onExitInteraction?.Invoke();
        }

        private void HandleItemDeliver(InventoryItem item)
        {
            if (_currentItemVisual) Destroy(_currentItemVisual);
            _currentItemVisual = Instantiate(item.Model, _itemVisualPosition);
            _currentItemVisual.transform.localScale = item.Model.transform.lossyScale;            
            _currentItemVisual.transform.localRotation = Quaternion.Euler(-_itemVisualPosition.transform.localRotation.eulerAngles.x, 0, -_itemVisualPosition.transform.localRotation.eulerAngles.z);
            _currentItem = item;
            IsActive = true;
            ExitInteraction();
            onActivate?.Invoke();
        }

        public void DeactivatePedestal()
        {
            if (IsActive)
            {
                IsActive = false;
                onActivate?.Invoke();
            }
        }
    }
}