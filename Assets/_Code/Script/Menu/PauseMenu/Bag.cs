using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Bag : MonoSingleton<Bag> {

        [Header("Parameters")]

        [SerializeField] private RectTransform _bagPagePrefab;
        private List<BagItem> _bagItemDisplays;
        [SerializeField] private GameObject _pageForwardBtn;
        [SerializeField] private GameObject _pageBackBtn;
        [SerializeField] private TextMeshProUGUI _itemDescriptor;

        [Header("Quick Open")]

        [SerializeField] private InputActionReference _quickOpenInput;
        [SerializeField] private Button _quickOpenBtn;

        [Header("Cache")]

        private MenuGroup _menuGroup;

        private Dictionary<int, BagItem> _itemBtnsByCurrentItem = new Dictionary<int, BagItem>();

        protected override void Awake() {
            base.Awake();

            if(TryGetComponent(out _menuGroup)) Debug.LogError($"Couldn't get {nameof(MenuGroup)} from '{name}'");
        }

        private void Start() {
            _quickOpenInput.action.performed += QuickOpen;
            PlayerInventory.Instance.onItemStackUpdate += HandleItemRemoved;
        }

        public void InventoryUpdate() {
            List<PlayerInventory.InventoryItemStack> bagDisplay = new List<PlayerInventory.InventoryItemStack>();
            foreach (PlayerInventory.InventoryItemStack inventoryItemStack in PlayerInventory.Instance.CheckInventory()) {
                if (inventoryItemStack.Item.Type != ItemType.Special) {
                    if (inventoryItemStack.Item.Type != ItemType.Special) bagDisplay.Add(inventoryItemStack);
                    else bagDisplay.Insert(0, inventoryItemStack);
                }
            }
            for (int i = 0; i < _bagItemDisplays.Count; i++) _bagItemDisplays[i].SetItemDisplay(i < bagDisplay.Count ? bagDisplay[i] : new PlayerInventory.InventoryItemStack());
        }

        public void PageInstantiate() {
            BagItem[] bagItemsNew = Instantiate(_bagPagePrefab, transform).GetComponentsInChildren<BagItem>();
            _bagItemDisplays.AddRange(bagItemsNew);
            ButtonEvents iterator;
            foreach (BagItem bagItem in bagItemsNew) {
                if (bagItem.TryGetComponent(out iterator)) {
                    iterator.OnSelectEvent.AddListener((data) => BtnSelectEvent(iterator.gameObject));
                    iterator.OnPointerEnterEvent.AddListener((data) => BtnPointerEnterEvent(iterator.gameObject));
                }
                else Debug.LogError($"Couldn't get {nameof(ButtonEvents)} from '{iterator.name}'");
            }
        }

        private void BtnSelectEvent(GameObject btn) {

        }

        private void BtnPointerEnterEvent(GameObject btn) {
            _menuGroup.SetSelected(btn);
        }

        public void DisplayItemInfo(InventoryItem item) {
            _itemDescriptor.text = item ? $"{item.GetDisplayName()}\n{item.GetDisplayDescription()}" : "";
        }

        public void UpdateItemDisplayText(InventoryItem item, string text) {
            //perhaps because of the lantern metter, the bag UI will need to have its own canvas for optimize draw call
            if (!PlayerInventory.Instance.CheckInventoryFor(item.name).Item) return;
            if (!_itemBtnsByCurrentItem.ContainsKey(item.GetInstanceID())) {
                for (int i = 0; i < _bagItemDisplays.Count; i++) {
                    if (_bagItemDisplays[i].Item == item && _bagItemDisplays[i].Item.DisplayTextFormatedExternaly) {
                        _itemBtnsByCurrentItem.Add(item.GetInstanceID(), _bagItemDisplays[i]);
                        break;
                    }
                }
            }
            if (_itemBtnsByCurrentItem.ContainsKey(item.GetInstanceID())) _itemBtnsByCurrentItem[item.GetInstanceID()].UpdateDisplayText(text);
        }

        private void QuickOpen(InputAction.CallbackContext context) {
            if (!Pause.Instance.Paused) {
                Pause.Instance.PauseGame(true);
                _quickOpenBtn.onClick.Invoke();
            }
        }

        //??
        private void HandleItemRemoved(PlayerInventory.InventoryItemStack itemStack) {
            if (_itemBtnsByCurrentItem.ContainsKey(itemStack.Item.GetInstanceID())) {
                _itemBtnsByCurrentItem.Remove(itemStack.Item.GetInstanceID());
            }
        }

    }
}