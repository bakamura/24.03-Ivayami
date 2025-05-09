using Ivayami.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using Ivayami.Save;

namespace Ivayami.UI
{
    [RequireComponent(typeof(Fade))]
    public class PlayerUseItemUI : MonoSingleton<PlayerUseItemUI>
    {
        [Header("Heal Callbacks")]
        public UnityEvent OnHealActivation;
        public UnityEvent OnHealEnd;
        public UnityEvent OnNotRequiredItem;
        public UnityEvent OnAlreadyHealing;
        public UnityEvent OnNotEnoughStressToHeal;

        [Header("General Callbacks")]
        public UnityEvent OnShowUI;
        public UnityEvent OnChangeOption;
        public UnityEvent OnHideUI;

        [Header("Components")]
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmOptionInput;
        [SerializeField] private InventoryItem[] _possibleOptions;
        [SerializeField] private LocalizedString _itemUsedText;

        private UseItemUIIcon _itemInDisplay;
        private Fade _fade;
        private Coroutine _currentItemActionCoroutine;
        private int _currentSelectedIndex;
        private bool _isActive;
        private bool _canOpen = true;

        public bool IsActive => _isActive;        

        protected override void Awake()
        {
            base.Awake();
            _itemInDisplay = GetComponentInChildren<UseItemUIIcon>();
            _fade = GetComponent<Fade>();
        }

        private void Start()
        {
            PlayerActions.Instance.onActionMapChange.AddListener(HandleInputMapChange);
            PlayerStress.Instance.onFail.AddListener(() => { if (IsActive) UpdateUI(false); });
            SavePoint.onSaveGameWithAnimation.AddListener(HandleOnSaveGameWithAnimation);
            SavePoint.onSaveSequenceEnd.AddListener(HandleOnSaveSequenceEnd);
        }
        /// <summary>
        /// Open And Closes the UI
        /// </summary>
        public void UpdateUI(bool isActive)
        {
            if (!_canOpen) return;
            _isActive = isActive;
            if (_isActive) OnShowUI?.Invoke();
            else OnHideUI?.Invoke();
            UpdateInputs();
            UpdateVisuals();
        }

        private void UpdateInputs()
        {
            if (_isActive)
            {
                _navigateUIInput.action.performed += HandleNavigateUI;
                _confirmOptionInput.action.started += HandleConfirmOption;
                PlayerActions.Instance.ToggleInteract(nameof(PlayerUseItemUI), false);
            }
            else
            {
                _navigateUIInput.action.performed -= HandleNavigateUI;
                _confirmOptionInput.action.started -= HandleConfirmOption;
                PlayerActions.Instance.ToggleInteract(nameof(PlayerUseItemUI), true);
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
            PlayerInventory.InventoryItemStack stack = PlayerInventory.Instance.CheckInventoryFor(_possibleOptions[_currentSelectedIndex].name);
            if (!stack.Item)
            {
                OnNotRequiredItem?.Invoke();
                UpdateUI(false);
                return;
            }
            else if (_currentItemActionCoroutine != null)
            {
                OnAlreadyHealing?.Invoke();
                UpdateUI(false);
                return;
            }
            else if (PlayerStress.Instance.StressCurrent == 0)
            {
                OnNotEnoughStressToHeal?.Invoke();
                UpdateUI(false);
                return;
            }

            _currentItemActionCoroutine = StartCoroutine(_possibleOptions[_currentSelectedIndex].UsageAction.ExecuteAtion(HandleItemActionEnd));
            PlayerInventory.Instance.RemoveFromInventory(_possibleOptions[_currentSelectedIndex]);
            InfoUpdateIndicator.Instance.DisplayUpdate(_possibleOptions[_currentSelectedIndex].Sprite, $"1 " +
                $"{stack.Item.GetDisplayName()} " +
                $"{_itemUsedText.GetLocalizedString()}");
            _isActive = false;
            UpdateInputs();
            UpdateVisuals();
            OnHealActivation?.Invoke();
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input.y != 0)
            {
                _currentSelectedIndex += input.y > 0 ? 1 : -1;
                LoopValueByArraySize(ref _currentSelectedIndex, _possibleOptions.Length);
                UpdateItemIcon();
                OnChangeOption?.Invoke();
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
            OnHealEnd?.Invoke();
        }

        private void HandleInputMapChange(string mapId)
        {
            CanOpenUI(string.Equals(mapId, "Player"));
        }

        private void HandleOnSaveSequenceEnd()
        {
            CanOpenUI(true);
        }

        private void HandleOnSaveGameWithAnimation()
        {
            CanOpenUI(false);
        }

        private void CanOpenUI(bool canOpen)
        {
            _canOpen = canOpen;
            if (!_canOpen && IsActive) UpdateUI(false);
        }
    }
}