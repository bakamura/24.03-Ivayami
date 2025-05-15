using Default;
using Ivayami.Player;
using Ivayami.Save;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace Ivayami.UI
{
    public class PlayerUseItemUI : MonoSingleton<PlayerUseItemUI>
    {
        [Header("Item Callbacks")]
        public UnityEvent OnItemActivation;
        public UnityEvent OnItemEffectEnd;
        public UnityEvent OnNotRequiredConsumableItem;
        public UnityEvent OnItemAlreadyInEffect;
        public UnityEvent OnItemActivationFail;
        //public UnityEvent OnNotEnoughStressToHeal;

        [Header("General Callbacks")]
        public UnityEvent OnShowUI;
        public UnityEvent OnChangeOption;
        public UnityEvent OnHideUI;

        [Header("Components")]
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmOptionInput;
        [SerializeField] private LocalizedString _itemUsedText;
        [SerializeField] private TMP_Text _itemSelectedDisplayName;
        [SerializeField] private ItemOption[] _possibleOptions;
        [SerializeField] private InventoryItem _itemConsumedOnUse;
        [SerializeField] private UseItemUIIcon _itemSelectedDisplay;
        [SerializeField] private UseItemUIIcon _nextItemDisplay;
        [SerializeField] private UseItemUIIcon _consumableItemDisplay;
        [SerializeField] private Fade _itemOptionsUI;
        [SerializeField] private Fade _itemInEffectUI;

        private Coroutine _currentItemActionCoroutine;
        private int _currentSelectedIndex;
        private bool _isActive;
        public HashKeyBlocker ActivateBlocker { get; private set; } = new HashKeyBlocker();

        [System.Serializable]
        private struct ItemOption
        {
            public InventoryItem Item;
            public UnityEvent OnActivation;
            public UnityEvent OnActivationFail;
        }

        public bool IsActive => _isActive;

        protected override void Awake()
        {
            base.Awake();
            _itemSelectedDisplay = GetComponentInChildren<UseItemUIIcon>();
            OnItemEffectEnd.AddListener(_itemInEffectUI.Close);
        }

        private void Start()
        {
            PlayerActions.Instance.onActionMapChange.AddListener(HandleInputMapChange);
            PlayerStress.Instance.onFail.AddListener(() => { if (IsActive) UpdateUI(false); });
            SavePoint.onSaveGameWithAnimation.AddListener(HandleOnSaveGameWithAnimation);
            SavePoint.onSaveSequenceEnd.AddListener(HandleOnSaveSequenceEnd);
            ActivateBlocker.OnToggleChange.AddListener(CanOpenUI);
        }
        /// <summary>
        /// Open And Closes the UI
        /// </summary>
        public void UpdateUI(bool isActive)
        {
            if (!ActivateBlocker.IsAllowed) return;
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
            if (_isActive)
            {
                _itemOptionsUI.Open();
                _itemInEffectUI.Close();
            }
            else
            {
                _itemOptionsUI.Close();
                if (_currentItemActionCoroutine != null) _itemInEffectUI.Open();
            }
            UpdateItemIcons();
        }

        private void UpdateItemIcons()
        {
            PlayerInventory.InventoryItemStack consumableStack = PlayerInventory.Instance.CheckInventoryFor(_itemConsumedOnUse.name);
            if (consumableStack.Item) _consumableItemDisplay.SetItemDisplay(consumableStack);
            else _consumableItemDisplay.SetItemDisplay(_itemConsumedOnUse);

            _itemSelectedDisplay.SetItemDisplay(_possibleOptions[_currentSelectedIndex].Item);
            _itemSelectedDisplayName.text = _possibleOptions[_currentSelectedIndex].Item.GetDisplayName();

            int nextIndex = _currentSelectedIndex + 1;
            LoopValueByArraySize(ref nextIndex, _possibleOptions.Length);
            _nextItemDisplay.SetItemDisplay(_possibleOptions[nextIndex].Item);
        }

        private void HandleConfirmOption(InputAction.CallbackContext context)
        {
            PlayerInventory.InventoryItemStack stack = PlayerInventory.Instance.CheckInventoryFor(_itemConsumedOnUse.name);
            if (!stack.Item)
            {
                OnNotRequiredConsumableItem?.Invoke();
                UpdateUI(false);
                return;
            }
            else if (_currentItemActionCoroutine != null)
            {
                OnItemAlreadyInEffect?.Invoke();
                UpdateUI(false);
                return;
            }
            //else if (PlayerStress.Instance.StressCurrent == 0)
            //{
            //    OnNotEnoughStressToHeal?.Invoke();
            //    UpdateUI(false);
            //    return;
            //}

            _currentItemActionCoroutine = StartCoroutine(_possibleOptions[_currentSelectedIndex].Item.UsageAction.ExecuteAtion(HanldeItemActionSuccess, HandleItemActionFail, HandleItemActionEnd));
            _isActive = false;
            UpdateInputs();
            UpdateVisuals();
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input.y != 0)
            {
                _currentSelectedIndex += input.y > 0 ? 1 : -1;
                LoopValueByArraySize(ref _currentSelectedIndex, _possibleOptions.Length);
                UpdateItemIcons();
                OnChangeOption?.Invoke();
            }
        }

        private void LoopValueByArraySize(ref int valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = arraySize - 1;
            else if (valueToConstrain >= arraySize) valueToConstrain = 0;
        }

        private void HanldeItemActionSuccess()
        {
            PlayerInventory.Instance.RemoveFromInventory(_itemConsumedOnUse);
            InfoUpdateIndicator.Instance.DisplayUpdate(_itemConsumedOnUse.Sprite, $"1 " +
                $"{_itemConsumedOnUse.GetDisplayName()} " +
                $"{_itemUsedText.GetLocalizedString()}");
            _possibleOptions[_currentSelectedIndex].OnActivation?.Invoke();
            OnItemActivation?.Invoke();
        }

        private void HandleItemActionFail()
        {
            _possibleOptions[_currentSelectedIndex].OnActivationFail?.Invoke();
            OnItemActivationFail?.Invoke();
        }

        private void HandleItemActionEnd()
        {
            _currentItemActionCoroutine = null;
            OnItemEffectEnd?.Invoke();
        }

        private void HandleInputMapChange(string mapId)
        {
            CanOpenUI(string.Equals(mapId, "Player"));
            ActivateBlocker.Toggle("InputMap", string.Equals(mapId, "Player"));
        }

        private void HandleOnSaveSequenceEnd()
        {
            CanOpenUI(true);
            ActivateBlocker.Toggle("Save", true);
        }

        private void HandleOnSaveGameWithAnimation()
        {
            CanOpenUI(false);
            ActivateBlocker.Toggle("Save", false);
        }

        private void CanOpenUI(bool canOpen)
        {
            if (!canOpen && IsActive) UpdateUI(false);
        }
    }
}