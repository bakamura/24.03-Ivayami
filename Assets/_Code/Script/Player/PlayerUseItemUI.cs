using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Ivayami.Player;

namespace Ivayami.UI
{
    [RequireComponent(typeof(Fade))]
    public class PlayerUseItemUI : MonoSingleton<PlayerUseItemUI>
    {
        [Header("Heal Callbacks")]
        [SerializeField] private UnityEvent _onHealActivation;
        [SerializeField] private UnityEvent _onHealEnd;
        [SerializeField] private UnityEvent _onNotRequiredItem;
        [SerializeField] private UnityEvent _onAlreadyHealing;
        [SerializeField] private UnityEvent _onNotEnoughStressToHeal;

        [Header("General Callbacks")]
        [SerializeField] private UnityEvent _onShowUI;
        [SerializeField] private UnityEvent _onChangeOption;

        [Header("Components")]
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmOptionInput;
        [SerializeField] private InventoryItem[] _possibleOptions;

        private UseItemUIIcon _itemInDisplay;
        private Fade _fade;
        private Coroutine _currentItemActionCoroutine;
        private int _currentSelectedIndex;
        private bool _isActive;

        protected override void Awake()
        {
            base.Awake();
            _itemInDisplay = GetComponentInChildren<UseItemUIIcon>();
            _fade = GetComponent<Fade>();
        }
        /// <summary>
        /// Open And Closes the UI
        /// </summary>
        public void UpdateUI()
        {
            _isActive = !_isActive;
            if (_isActive) _onShowUI?.Invoke();
            UpdateInputs();
            UpdateVisuals();
        }

        private void UpdateInputs()
        {
            if (_isActive)
            {
                _navigateUIInput.action.performed += HandleNavigateUI;
                _confirmOptionInput.action.started += HandleConfirmOption;
            }
            else
            {
                _navigateUIInput.action.performed -= HandleNavigateUI;
                _confirmOptionInput.action.started -= HandleConfirmOption;
            }
        }

        private void UpdateVisuals()
        {
            PlayerAnimation.Instance.UseMP3(_isActive);
            if (_isActive) _fade.Open();
            else _fade.Close();
            UpdateItemIcon();
        }

        private void UpdateItemIcon()
        {
            PlayerInventory.InventoryItemStack stack = PlayerInventory.Instance.CheckInventoryFor(_possibleOptions[_currentSelectedIndex].name);
            if (stack.Item) _itemInDisplay.SetItemDisplay(stack);            
            else _itemInDisplay.SetItemDisplay(_possibleOptions[_currentSelectedIndex]);
        }

        private void HandleConfirmOption(InputAction.CallbackContext context)
        {
            if (!PlayerInventory.Instance.CheckInventoryFor(_possibleOptions[_currentSelectedIndex].name).Item)
            {
                _onNotRequiredItem?.Invoke();
                UpdateUI();
                return;
            }
            else if (_currentItemActionCoroutine != null)
            {
                _onAlreadyHealing?.Invoke();
                UpdateUI();
                return;
            }
            else if (PlayerStress.Instance.StressCurrent == 0)
            {
                _onNotEnoughStressToHeal?.Invoke();
                UpdateUI();
                return;
            }

            _currentItemActionCoroutine = StartCoroutine(_possibleOptions[_currentSelectedIndex].UsageAction.ExecuteAtion(HandleItemActionEnd));
            PlayerInventory.Instance.RemoveFromInventory(_possibleOptions[_currentSelectedIndex]);
            InfoUpdateIndicator.Instance.DisplayUpdate(_possibleOptions[_currentSelectedIndex].Sprite, "-1");
            _isActive = false;
            UpdateInputs();
            UpdateVisuals();
            _onHealActivation?.Invoke();
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input.y != 0)
            {
                _currentSelectedIndex += input.y > 0 ? 1 : -1;
                LoopValueByArraySize(ref _currentSelectedIndex, _possibleOptions.Length);
                UpdateItemIcon();
                _onChangeOption?.Invoke();
            }
        }

        private void LoopValueByArraySize(ref int valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = arraySize - 1;
            else if (valueToConstrain >= arraySize) valueToConstrain = 0;
        }

        private void HandleItemActionEnd()
        {
            _currentItemActionCoroutine = null;
            _onHealEnd?.Invoke();
        }
    }
}