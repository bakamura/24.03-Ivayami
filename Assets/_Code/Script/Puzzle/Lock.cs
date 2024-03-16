using UnityEngine;
using Paranapiacaba.Player;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Paranapiacaba.Puzzle
{
    public class Lock : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionAsset _inputActionMap;
        [SerializeField] private InteractionTypes _interactionType;
        [SerializeField, Tooltip("if empty, will not require it")] private InventoryItem[] _itemsRequired;
        [SerializeField] private CanvasGroup _deliverItemsUI;
        [SerializeField, Tooltip("if null, will not require it")] private string _passwordRequired;
        [SerializeField] private TMP_InputField _passwordTextField;
        [SerializeField] private CanvasGroup _passwordUI;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;

        [System.Serializable]
        private enum InteractionTypes
        {
            RequireItems,
            RequirePassword
        }
        private Button _initialPasswordButton;

        private void Awake()
        {
            _cancelInteractionInput.action.performed += HandleExitInteraction;
            _initialPasswordButton = _passwordUI.GetComponentInChildren<Button>();
        }

        public void Interact()
        {
            _onInteract?.Invoke();
            UpdateInputs(true);
            if (_interactionType == InteractionTypes.RequirePassword)
            {
                UpdatePasswordUI(true);                
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
                        break;
                    }
                }
            }
            bool isPasswordCorrect = string.IsNullOrEmpty(_passwordRequired) || _passwordRequired == _passwordTextField.text;
            if (hasItems && isPasswordCorrect)
            {
                UpdatePasswordUI(false);
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

        private void UpdatePasswordUI(bool isActive)
        {            
            _passwordUI.alpha = isActive ? 1 : 0;
            _passwordUI.interactable = isActive;
            _passwordUI.blocksRaycasts = isActive;
            if(isActive) EventSystem.current.SetSelectedGameObject(_initialPasswordButton.gameObject);
        }

        public void InsertCharacter(TMP_Text text)
        {
            _passwordTextField.text += text.text;
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
            UpdatePasswordUI(false);
            UpdateDeliverItemUI(false);
            UpdateInputs(false);
            _onCancelInteraction?.Invoke();
        }

        private void OnValidate()
        {
            if (_passwordTextField)
            {
                _passwordTextField.characterLimit = _passwordRequired.Length;
            }
        }

    }
}