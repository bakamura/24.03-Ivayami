using Ivayami.Audio;
using Ivayami.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Ivayami.Save;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(LockPuzzleSounds), typeof(CanvasGroup))]
    public class DeliverUI : MonoBehaviour
    {
        [SerializeField] private InputActionReference _navigateUIInput;
        //[SerializeField] private InputActionReference _deliverInput;
        [SerializeField] private byte _requestAmountToComplete = 1;
        [SerializeField] private bool _skipDeliverUI;
        [SerializeField] private bool _deliverAnyItem;
        [SerializeField] private ItemRequestData[] _itemsRequired;
        //[SerializeField, Tooltip("Needs to always contain an odd number off child objects")] private RectTransform _deliverOptionsContainer;
        [SerializeField] private Image[] _deliverItemOptionsIcon;
        [SerializeField] private TMP_Text _itemDisplayName;
        [SerializeField] private Selectable _deliverBtn;
        [SerializeField] private UnityEvent<bool> _onTryDeliver;

        private List<ItemRequestData> _currentRequests = new List<ItemRequestData>();
        private List<InventoryItem> _itemsDelivered = new List<InventoryItem>();
        private List<InventoryItem> _itemsCache;
        private CanvasGroup _deliverItemsUI;
        private LockPuzzleSounds _lockSounds;
        private InventoryItem _currentItemSelected;
        private int _currentRequestIndex = 0;
        private const float _navigateInputCooldown = .2f;
        private float _navigateInputCurrentCooldown;

        [HideInInspector] public UnityEvent<InventoryItem> OnDeliver;
        public bool SkipDeliverUI => _skipDeliverUI;
        [System.Serializable]
        private struct ItemRequestData
        {
            public InventoryItem Item;
            public bool UseItem;
            public UnityEvent OnItemDelivered;
        }

        private void Awake()
        {
            _lockSounds = GetComponent<LockPuzzleSounds>();
            _deliverItemsUI = GetComponent<CanvasGroup>();
            if (_itemsRequired != null && _currentRequests.Count == 0)
            {
                for (int i = 0; i < _itemsRequired.Length; i++)
                {
                    _currentRequests.Add(_itemsRequired[i]);
                }
            }
        }

        public void UpdateUI(bool isActive)
        {
            if (isActive)
            {
                if (_skipDeliverUI)
                {
                    DeliverItem();
                    return;
                }
                UpdateInputs(true);
                UpdateDeliverItemUI(true);
            }
            else
            {
                UpdateInputs(false);
                UpdateDeliverItemUI(false);
            }
        }

        //called by interface Btn
        public void DeliverItem()
        {
            bool deliverAchived = false;
            if (_skipDeliverUI && !_deliverAnyItem)
            {
                for (int i = 0; i < _currentRequests.Count; i++)
                {
                    if (PlayerInventory.Instance.CheckInventoryFor(_currentRequests[i].Item.name).Item)
                    {
                        deliverAchived = true;
                        OnDeliver?.Invoke(_currentRequests[i].Item);
                        RemoveItemFromRequestList(_currentRequests[i].Item);
                    }
                }
            }
            else
            {
                _lockSounds.PlaySound(LockPuzzleSounds.SoundTypes.ConfirmOption);
                bool isInRequestList = _deliverAnyItem ? true : _currentRequests.Find(x => x.Item == _currentItemSelected).Item;
                if (isInRequestList && PlayerInventory.Instance.CheckInventoryFor(_currentItemSelected.name).Item)
                {
                    RemoveItemFromRequestList(_currentItemSelected);
                    ConstrainValueToArraySize(ref _currentRequestIndex, _itemsCache.Count);
                    if (_currentRequests.Count > 0) UpdateDeliverIcons((byte)_currentRequestIndex);
                    deliverAchived = true;
                    OnDeliver?.Invoke(_currentItemSelected);
                }
            }
            _onTryDeliver?.Invoke(deliverAchived);
        }

        public bool CheckRequestsCompletion()
        {
            return _itemsRequired.Length - _currentRequests.Count >= _requestAmountToComplete;
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _navigateUIInput.action.performed += HandleNavigateUI;
                //_deliverInput.action.started += HandleDeliverInput;
                PlayerMovement.Instance.ToggleMovement(nameof(DeliverUI), false);
                PlayerActions.Instance.ChangeInputMap("Menu");
                PlayerActions.Instance.ToggleInteract(nameof(DeliverUI), false);
                //StartCoroutine(ActivateInputCoroutine());
            }
            else
            {
                _navigateUIInput.action.performed -= HandleNavigateUI;
                //_deliverInput.action.started -= HandleDeliverInput;
                PlayerMovement.Instance.ToggleMovement(nameof(DeliverUI), true);
                PlayerActions.Instance.ChangeInputMap("Player");
                PlayerActions.Instance.ToggleInteract(nameof(DeliverUI), true);
            }
        }        

        //private IEnumerator ActivateInputCoroutine()
        //{
        //    yield return new WaitForEndOfFrame();
        //    _deliverInput.action.Enable();
        //}

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input.x != 0 && Time.time - _navigateInputCurrentCooldown > _navigateInputCooldown)
            {
                _lockSounds.PlaySound(LockPuzzleSounds.SoundTypes.ChangeOption);
                int temp = input.x > 0 ? 1 : -1;
                _currentRequestIndex += temp;
                LoopValueByArraySize(ref _currentRequestIndex, _itemsCache.Count);
                UpdateDeliverIcons(_currentRequestIndex);
                _navigateInputCurrentCooldown = Time.time;
            }
        }

        //private void HandleDeliverInput(InputAction.CallbackContext obj)
        //{
        //    DeliverItem();
        //}

        private void UpdateDeliverItemUI(bool isActive)
        {
            _deliverItemsUI.alpha = isActive ? 1 : 0;
            _deliverItemsUI.interactable = isActive;
            _deliverItemsUI.blocksRaycasts = isActive;

            if (isActive)
            {
                _currentRequestIndex = 0;
                //select only the items that match with items requests types
                _itemsCache = PlayerInventory.Instance.CheckInventory().Where(x => ContainItemTypeInRequest(x.Item.Type)).Select(x => x.Item).ToList();
                //add any missing items
                for (int i = 0; i < _currentRequests.Count; i++)
                {
                    if (!_itemsCache.Contains(_currentRequests[i].Item))
                        _itemsCache.Add(_currentRequests[i].Item);
                }
                if (_itemsDelivered.Count > 0) _itemsCache.RemoveAll(x => _itemsDelivered.Contains(x));
                UpdateDeliverIcons(0);
                _deliverBtn.Select();
            }
        }

        private void UpdateDeliverIcons(int startIndex)
        {
            for (int i = 0; i < _deliverItemOptionsIcon.Length; i++)
            {
                _deliverItemOptionsIcon[i].enabled = false;
            }
            int iconsIndex = _itemsCache.Count < _deliverItemOptionsIcon.Length ? Mathf.FloorToInt(_deliverItemOptionsIcon.Length / 2) : 0;
            int iconsFilled = 0;
            int requestIndex = startIndex;
            _currentItemSelected = null;
            while (iconsFilled < _deliverItemOptionsIcon.Length && iconsFilled < _itemsCache.Count)
            {
                if (requestIndex < _itemsCache.Count)
                {
                    _deliverItemOptionsIcon[iconsIndex].enabled = true;
                    _deliverItemOptionsIcon[iconsIndex].sprite = PlayerInventory.Instance.CheckInventoryFor(_itemsCache[requestIndex].name).Item ? 
                        _itemsCache[requestIndex].Sprite : PlayerInventory.Instance.ItemTypeDefaultIcons[_itemsCache[requestIndex].Type];
                    if (iconsIndex == Mathf.FloorToInt(_deliverItemOptionsIcon.Length / 2)
                        && !_currentItemSelected) _currentItemSelected = _itemsCache[requestIndex];
                }
                iconsIndex++;
                iconsFilled++;
                requestIndex++;
                if (requestIndex == _itemsCache.Count) requestIndex = 0;
            }
            _itemDisplayName.text = PlayerInventory.Instance.CheckInventoryFor(_currentItemSelected.name).Item ? 
                _currentItemSelected.GetDisplayName() : null;
        }

        private bool ContainItemTypeInRequest(ItemType itemType)
        {
            for (int i = 0; i < _currentRequests.Count; i++)
            {
                if (_currentRequests[i].Item.Type == itemType) return true;
            }
            return false;
        }

        private void RemoveItemFromRequestList(InventoryItem item)
        {
            int index;
            bool itemFound = false;
            for (index = 0; index < _currentRequests.Count; index++)
            {
                if (_currentRequests[index].Item == item)
                {
                    itemFound = true;
                    break;
                }
            }
            if (itemFound)
            {
                _currentRequests[index].OnItemDelivered?.Invoke();
                if (_currentRequests[index].UseItem) PlayerInventory.Instance.RemoveFromInventory(_currentRequests[index].Item);
                _currentRequests.RemoveAt(index);
                _itemsDelivered.Add(item);
                _itemsCache?.Remove(item);
            }
            else
            {
                PlayerInventory.Instance.RemoveFromInventory(item);
            }
        }

        private void LoopValueByArraySize(ref int valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = arraySize - 1;
            else if (valueToConstrain >= arraySize) valueToConstrain = 0;
        }

        private void ConstrainValueToArraySize(ref int valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = 0;
            else if (valueToConstrain >= arraySize) valueToConstrain = arraySize - 1;
        }

        public void RevertItemDeliver(InventoryItem item)
        {
            if (_itemsDelivered.Contains(item)) _itemsDelivered.Remove(item);
            for (int i = 0; i < _itemsRequired.Length; i++)
            {
                if (_itemsRequired[i].Item == item)
                {
                    if (!_currentRequests.Contains(_itemsRequired[i])) _currentRequests.Add(_itemsRequired[i]);
                    break;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_itemsRequired == null) return;
            if (_requestAmountToComplete > _itemsRequired.Length) _requestAmountToComplete = (byte)_itemsRequired.Length;
        }
#endif
    }
}
