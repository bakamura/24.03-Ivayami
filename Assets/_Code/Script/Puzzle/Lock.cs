using UnityEngine;
using Paranapiacaba.Player;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Paranapiacaba.Puzzle
{
    public class Lock : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionReference _navigateUIInput;
        //[SerializeField] private InputActionAsset _inputActionMap;

        [SerializeField] private InteractionTypes _interactionType;

        [SerializeField] private ItemRequestData[] _itemsRequired;
        [SerializeField] private CanvasGroup _deliverItemsUI;
        [SerializeField, Tooltip("Needs to always contain an odd number off child objects")] private RectTransform _deliverOptionsContainer;        
        [SerializeField] private GameObject _deliverBtn;

        [SerializeField] private PasswordUI _passwordUI;

        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;


        private int _selectedDeliverOptionIndex;
        private Image[] _deliverOptions;
        private int _currentPositionInInventory = 0;
        private List<InventoryItem> _currentItemList = new List<InventoryItem>();
        private sbyte _currentItemsDelivered;

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
            public UnityEvent OnItemDelivered;
            [HideInInspector] public bool ItemDelivered;
        }

        private void Awake()
        {            
            _deliverOptions = _deliverOptionsContainer.GetComponentsInChildren<Image>();
        }

        [ContextMenu("Interact")]
        public bool Interact()
        {
            _onInteract?.Invoke();
            UpdateInputs(true);
            if (_interactionType == InteractionTypes.RequirePassword)
            {
                _passwordUI.UpdateActiveState(true);
            }
            else
            {
                UpdateDeliverItemUI(true);
            }
            return false;
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
                onActivate?.Invoke();
            }
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _cancelInteractionInput.action.performed += HandleExitInteraction;
                _navigateUIInput.action.performed += HandleNavigateDeliverUI;
                PlayerActions.Instance.ChangeInputMap("Menu");
                ////gameplay actions
                //_inputActionMap.actionMaps[0].Disable();
                ////ui actions
                //_inputActionMap.actionMaps[1].Enable();                
            }
            else
            {
                _cancelInteractionInput.action.performed -= HandleExitInteraction;
                _navigateUIInput.action.performed -= HandleNavigateDeliverUI;
                PlayerActions.Instance.ChangeInputMap("Player");
                ////gameplay actions
                //_inputActionMap.actionMaps[0].Enable();
                ////ui actions
                //_inputActionMap.actionMaps[1].Disable();
            }
        }
        
        public void CancelInteraction()
        {
            _passwordUI.UpdateActiveState(false);
            UpdateDeliverItemUI(false);
            UpdateInputs(false);
            _onCancelInteraction?.Invoke();
        }

        private void HandleExitInteraction(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                CancelInteraction();
            }
        }

        private void HandleNavigateDeliverUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input != Vector2.zero)
            {
                switch (_interactionType)
                {
                    case InteractionTypes.RequirePassword:
                        if(EventSystem.current.currentSelectedGameObject == null)                    
                            EventSystem.current.SetSelectedGameObject(_passwordUI.FallbackButton.gameObject);                    
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

        #region DeliverUI

        private void UpdateDeliverItemUI(bool isActive)
        {
            _deliverItemsUI.alpha = isActive ? 1 : 0;
            _deliverItemsUI.interactable = isActive;
            _deliverItemsUI.blocksRaycasts = isActive;

            if (isActive)
            {
                _currentPositionInInventory = 0;
                _selectedDeliverOptionIndex = Mathf.FloorToInt(_deliverOptions.Length / 2);
                _currentItemList = PlayerInventory.Instance.CheckInventory().Where(x => CheckItemType(x.type)).ToList();
                for (int i = 0; i < _deliverOptions.Length; i++)
                {
                    if (i == _currentItemList.Count) break;
                    _deliverOptions[i].sprite = _currentItemList[i].sprite;
                }
                EventSystem.current.SetSelectedGameObject(_deliverBtn);
            }
        }

        private bool CheckItemType(ItemType itemType)
        {
            for(int i = 0; i < _itemsRequired.Length; i++)
            {
                if (_itemsRequired[i].Item.type == itemType)
                    return true;
            }
            return false;
        }                

        //called by interface Btn
        public void DeliverItem()
        {
            for(int i = 0; i < _itemsRequired.Length; i++)
            {                
                if(_itemsRequired[i].Item.name == _currentItemList[_selectedDeliverOptionIndex].name && !_itemsRequired[i].ItemDelivered)
                {
                    _itemsRequired[i].ItemDelivered = true;
                    _itemsRequired[i].OnItemDelivered?.Invoke();
                    PlayerInventory.Instance.RemoveFromInventory(_currentItemList[_selectedDeliverOptionIndex]);
                    _currentItemsDelivered++;
                    TryUnlock();
                    break;
                }
            }
        }

        private void UpdateInventoryIcons()
        {
            _currentPositionInInventory = ConstrainValueToArrayBounds(_currentPositionInInventory, _currentItemList.Count);
            _selectedDeliverOptionIndex = ConstrainValueToArrayBounds(_selectedDeliverOptionIndex, _currentItemList.Count);

            sbyte currentDeliverIndex = 0;
            int currentInventoryListIndex = _currentPositionInInventory;
            while (currentDeliverIndex < _deliverOptions.Length)
            {
                _deliverOptions[currentDeliverIndex].sprite = _currentItemList[currentInventoryListIndex].sprite;
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