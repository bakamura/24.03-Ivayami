using UnityEngine;
using Paranapiacaba.Player;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

namespace Paranapiacaba.Puzzle
{
    public class Lock : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionAsset _inputActionMap;

        [SerializeField] private InteractionTypes _interactionType;

        [SerializeField] private InventoryItem[] _itemsRequired;
        [SerializeField] private CanvasGroup _deliverItemsUI;
        [SerializeField] private GameObject _deliverItemBtnPrefab;

        [SerializeField] private PasswordUI _passwordUI;

        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;

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
        }

        private TMP_Text _deliverItemFeedbackText;

        private void Awake()
        {
            _cancelInteractionInput.action.performed += HandleExitInteraction;            
            _deliverItemFeedbackText = _deliverItemsUI.GetComponentInChildren<TMP_Text>();
        }

        [ContextMenu("Interact")]
        public void Interact()
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
        }

        public void TryUnlock()
        {
            bool hasItems = true;
            if (_itemsRequired.Length > 0)
            {
                for (int i = 0; i < _itemsRequired.Length; i++)
                {
                    if (!PlayerInventory.Instance.CheckInventoryFor(_itemsRequired[i].id))
                    {
                        hasItems = false;
                        _deliverItemFeedbackText.text = "DONT HAVE REQUIRED ITEMS";
                        break;
                    }
                }
            }

            if (hasItems && _passwordUI.CheckPassword())
            {
                _passwordUI.UpdateActiveState(false);
                UpdateDeliverItemUI(false);
                UpdateInputs(false);
                onActivate?.Invoke();
            }
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                //gameplay actions
                _inputActionMap.actionMaps[0].Disable();
                //ui actions
                _inputActionMap.actionMaps[1].Enable();
            }
            else
            {
                //gameplay actions
                _inputActionMap.actionMaps[0].Enable();
                //ui actions
                _inputActionMap.actionMaps[1].Disable();
            }
        }

        private void UpdateDeliverItemUI(bool isActive)
        {
            _deliverItemsUI.alpha = isActive ? 1 : 0;
            _deliverItemsUI.interactable = isActive;
            _deliverItemsUI.blocksRaycasts = isActive;
        }

        private void HandleExitInteraction(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                CancelInteraction();
            }
        }

        public void CancelInteraction()
        {
            _passwordUI.UpdateActiveState(false);
            UpdateDeliverItemUI(false);
            UpdateInputs(false);
            _onCancelInteraction?.Invoke();
        }
    }
}