using UnityEngine;
using Ivayami.Player;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Ivayami.Audio;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
    public class Lock : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmInput;

        [SerializeField] private InteractionTypes _interactionType;

        [SerializeField] private ItemRequestData[] _itemsRequired;
        [SerializeField] private CanvasGroup _deliverItemsUI;
        [SerializeField, Tooltip("Needs to always contain an odd number off child objects")] private RectTransform _deliverOptionsContainer;
        [SerializeField] private GameObject _deliverBtn;
        [SerializeField] private UnityEvent _onItemDeliverFailed;

        [SerializeField] private PasswordUI _passwordUI;

        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;
        [SerializeField] private UnityEvent _onInteractionFailed;


        private int _selectedDeliverOptionIndex;
        private Image[] _deliverOptions;
        private int _currentPositionInInventory = 0;
        private List<InventoryItem> _currentItemList = new List<InventoryItem>();
        private sbyte _currentItemsDelivered;
        private InteractableFeedbacks _interatctableFeedbacks;
        private InteractableSounds _interactableSounds;
        public InteractableFeedbacks InteratctableHighlight { get => _interatctableFeedbacks; }

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
            [HideInInspector] public bool ItemDelivered;
        }

        private void Awake()
        {
            _deliverOptions = _deliverOptionsContainer.GetComponentsInChildren<Image>();
            _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
        }

        [ContextMenu("Interact")]
        public PlayerActions.InteractAnimation Interact()
        {
            _onInteract?.Invoke();
            UpdateInputs(true);
            _interatctableFeedbacks.UpdateFeedbacks(false);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            if (_interactionType == InteractionTypes.RequirePassword)
            {
                _passwordUI.UpdateActiveState(true);
            }
            else
            {
                UpdateDeliverItemUI(true);
            }
            return PlayerActions.InteractAnimation.Default;
        }

        public void TryUnlock()
        {
            bool isPasswordCorrect = _interactionType == InteractionTypes.RequirePassword && _passwordUI.CheckPassword();
            bool hasDeliveredAllItems = _interactionType == InteractionTypes.RequireItems && _itemsRequired.Length == _currentItemsDelivered;
            if (hasDeliveredAllItems || isPasswordCorrect)
            {
                _passwordUI.UpdateActiveState(false);
                UpdateDeliverItemUI(false);
                UpdateInputs(false);
                IsActive = !IsActive;
                _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionSuccess);
                onActivate?.Invoke();
            }
            else
            {
                _onInteractionFailed?.Invoke();
                _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionFailed);
            }
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _cancelInteractionInput.action.performed += HandleExitInteraction;
                _navigateUIInput.action.performed += HandleNavigateUI;
                if (_passwordUI)
                {
                    _passwordUI.OnCheckPassword += TryUnlock;
                    if (_passwordUI is RotateLock) _confirmInput.action.performed += HandleConfirmUI;
                }
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _cancelInteractionInput.action.performed -= HandleExitInteraction;
                _navigateUIInput.action.performed -= HandleNavigateUI;
                if (_passwordUI)
                {
                    _passwordUI.OnCheckPassword -= TryUnlock;
                    if (_passwordUI is RotateLock) _confirmInput.action.performed -= HandleConfirmUI;
                }
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }

        public void CancelInteraction()
        {
            _passwordUI.UpdateActiveState(false);
            UpdateDeliverItemUI(false);
            UpdateInputs(false);
            _interatctableFeedbacks.UpdateFeedbacks(true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.InteractReturn);
            _onCancelInteraction?.Invoke();
        }

        private void HandleExitInteraction(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                CancelInteraction();
            }
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input != Vector2.zero)
            {
                switch (_interactionType)
                {
                    case InteractionTypes.RequirePassword:
                        if (EventSystem.current.currentSelectedGameObject == null)
                            EventSystem.current.SetSelectedGameObject(_passwordUI.FallbackButton);
                        break;
                    case InteractionTypes.RequireItems:
                        if (EventSystem.current.currentSelectedGameObject == null)
                            EventSystem.current.SetSelectedGameObject(_deliverBtn);
                        else if (input.y != 0)
                        {
                            int temp = -input.y > 0 ? 1 : -1;
                            _currentPositionInInventory += temp;
                            _selectedDeliverOptionIndex += temp;
                            UpdateInventoryIcons();
                        }
                        break;
                }
            }
        }

        private void HandleConfirmUI(InputAction.CallbackContext context)
        {
            TryUnlock();
        }

        #region DeliverUI

        private void UpdateDeliverItemUI(bool isActive)
        {
            _deliverItemsUI.alpha = isActive ? 1 : 0;
            _deliverItemsUI.interactable = isActive;
            _deliverItemsUI.blocksRaycasts = isActive;

            if (isActive)
            {
                _currentPositionInInventory = 0;
                if (_itemsRequired.Length < Mathf.Floor(_deliverOptions.Length / 2))
                    _selectedDeliverOptionIndex = Mathf.FloorToInt(_itemsRequired.Length / Mathf.Ceil(_deliverOptions.Length / 2f));
                else
                    _selectedDeliverOptionIndex = Mathf.FloorToInt(_deliverOptions.Length / 2);

                if (_currentItemList.Count == 0)
                {
                    for (int i = 0; i < _itemsRequired.Length; i++)
                    {
                        _currentItemList.Add(_itemsRequired[i].Item);
                    }
                }
                for (int i = 0; i < _deliverOptions.Length; i++)
                {
                    if (i >= _itemsRequired.Length)
                    {
                        _deliverOptions[i].sprite = _itemsRequired[^1].Item.Sprite;
                    }
                    else _deliverOptions[i].sprite = _itemsRequired[i].Item.Sprite;
                }
                EventSystem.current.SetSelectedGameObject(_deliverBtn);
            }
        }

        //called by interface Btn
        public void DeliverItem()
        {
            for (int i = 0; i < _itemsRequired.Length; i++)
            {
                if (_itemsRequired[i].Item.name == _currentItemList[_selectedDeliverOptionIndex].name
                    && PlayerInventory.Instance.CheckInventoryFor(_currentItemList[_selectedDeliverOptionIndex].name)
                    && !_itemsRequired[i].ItemDelivered)
                {
                    _itemsRequired[i].ItemDelivered = true;
                    _itemsRequired[i].OnItemDelivered?.Invoke();
                    if (_itemsRequired[i].UseItem) PlayerInventory.Instance.RemoveFromInventory(_currentItemList[_selectedDeliverOptionIndex]);
                    _currentItemsDelivered++;
                    TryUnlock();
                    return;
                }
            }
            _onItemDeliverFailed?.Invoke();
        }

        private void UpdateInventoryIcons()
        {
            _currentPositionInInventory = ConstrainValueToArrayBounds(_currentPositionInInventory, _currentItemList.Count);
            _selectedDeliverOptionIndex = ConstrainValueToArrayBounds(_selectedDeliverOptionIndex, _currentItemList.Count);

            sbyte currentDeliverIndex = 0;
            int currentInventoryListIndex = _currentPositionInInventory;
            while (currentDeliverIndex < _deliverOptions.Length)
            {
                _deliverOptions[currentDeliverIndex].sprite = _currentItemList[currentInventoryListIndex].Sprite;
                currentDeliverIndex++;
                currentInventoryListIndex++;
                currentInventoryListIndex = ConstrainValueToArrayBounds(currentInventoryListIndex, _currentItemList.Count);
            }
        }

        private int ConstrainValueToArrayBounds(int valueToConstrain, int listSize)
        {
            if (valueToConstrain < 0) valueToConstrain = listSize - 1;
            else if (valueToConstrain == listSize) valueToConstrain = 0;
            return valueToConstrain;
        }
        #endregion
    }
}