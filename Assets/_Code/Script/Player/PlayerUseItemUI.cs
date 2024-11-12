using UnityEngine;
using UnityEngine.InputSystem;
using Ivayami.UI;

namespace Ivayami.Player
{
    [RequireComponent(typeof(Fade))]
    public class PlayerUseItemUI : MonoSingleton<PlayerUseItemUI>
    {
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmOptionInput;
        [SerializeField] private InventoryItem[] _possibleOptions;

        private BagItem _itemInDisplay;
        private Fade _fade;
        private Coroutine _currentItemActionCoroutine;
        private int _currentSelectedIndex;
        private bool _isActive;

        protected override void Awake()
        {
            base.Awake();
            _itemInDisplay = GetComponentInChildren<BagItem>();
            _fade = GetComponent<Fade>();            
        }
        /// <summary>
        /// Open And Closes the UI
        /// </summary>
        public void UpdateUI()
        {
            if (_currentItemActionCoroutine != null || PlayerStress.Instance.StressCurrent == 0) return;
            _isActive = !_isActive;
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
            if(_isActive) _fade.Open();
            else _fade.Close();
            _itemInDisplay.SetItemDisplay(PlayerInventory.Instance.CheckInventoryFor(_possibleOptions[_currentSelectedIndex].name));
        }

        private void HandleConfirmOption(InputAction.CallbackContext context)
        {
            if (PlayerInventory.Instance.CheckInventoryFor(_possibleOptions[_currentSelectedIndex].name).Item)
            {
                _currentItemActionCoroutine = StartCoroutine(_possibleOptions[_currentSelectedIndex].UsageAction.ExecuteAtion(HandleItemActionEnd));
                PlayerInventory.Instance.RemoveFromInventory(_possibleOptions[_currentSelectedIndex]);
                InfoUpdateIndicator.Instance.DisplayUpdate(_possibleOptions[_currentSelectedIndex].Sprite, "-1");
                _isActive = false;
                UpdateInputs();
                UpdateVisuals();
            }
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if(input.y != 0)
            {
                _currentSelectedIndex += input.y > 0 ? 1 : -1;
                LoopValueByArraySize(ref _currentSelectedIndex, _possibleOptions.Length);
                _itemInDisplay.SetItemDisplay(PlayerInventory.Instance.CheckInventoryFor(_possibleOptions[_currentSelectedIndex].name));
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
        }
    }
}