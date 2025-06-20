using Ivayami.Audio;
using Ivayami.Player;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class Bag : MonoSingleton<Bag> {

        [Header("Parameters")]

        [SerializeField] private RectTransform _bagPagePrefab;
        private List<Menu> _pages = new List<Menu>();
        private int _pageCurrent;
        private int _pageLimitCurrent;
        private List<BagItem> _bagItemDisplays = new List<BagItem>();
        [SerializeField] private Button _pageForwardBtn;
        [SerializeField] private Button _pageBackBtn;
        [SerializeField] private TextMeshProUGUI _itemDescriptor;

        [Header("Quick Open")]

        [SerializeField] private InputActionReference _quickOpenInput;
        [SerializeField] private Button _quickOpenBtn;

        [Header("Cache")]

        private MenuGroup _menuGroup;
        private HighlightGroup _highlightGroup;
        private UiSound _uiSound;

        private Dictionary<int, BagItem> _itemBtnsByCurrentItem = new Dictionary<int, BagItem>();

        protected override void Awake() {
            base.Awake();

            if (!TryGetComponent(out _menuGroup)) Debug.LogError($"Couldn't get {nameof(MenuGroup)} from '{name}'");
            if (!TryGetComponent(out _highlightGroup)) Debug.LogError($"Couldn't get {nameof(HighlightGroup)} from '{name}'");
            if (!TryGetComponent(out _uiSound)) Debug.LogError($"Couldn't get {nameof(UiSound)} from '{name}'");
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
            while (_bagItemDisplays.Count < bagDisplay.Count) PageInstantiate(); //
            ChangePage(0);
            for (int i = 0; i < _bagItemDisplays.Count; i++) _bagItemDisplays[i].SetItemDisplay(i < bagDisplay.Count ? bagDisplay[i] : new PlayerInventory.InventoryItemStack());
            _pageLimitCurrent = (bagDisplay.Count / _bagPagePrefab.childCount) - 1;
        }

        public void PageInstantiate() {
            _pages.Add(Instantiate(_bagPagePrefab, transform).GetComponent<Menu>());
            BagItem[] bagItemsNew = _pages[_pages.Count - 1].GetComponentsInChildren<BagItem>();
            _bagItemDisplays.AddRange(bagItemsNew);
            foreach (BagItem bagItem in bagItemsNew) {
                if (bagItem.TryGetComponent(out ButtonEvents btnEvents)) {
                    btnEvents.OnSelectEvent.AddListener((data) => BtnSelectEvent(bagItem));
                    btnEvents.OnPointerEnterEvent.AddListener((data) => BtnPointerEnterEvent(btnEvents.gameObject));
                }
                else Debug.LogError($"Couldn't get {nameof(ButtonEvents)} from '{btnEvents.name}'");
            }
        }

        private void BtnSelectEvent(BagItem bagItem) {
            _highlightGroup.SetHighlightTo(bagItem.Highlightable);
            _uiSound.ChangeSelected();
        }

        private void BtnPointerEnterEvent(GameObject btn) {
            _menuGroup.SetSelected(btn);
        }

        private void ChangePage(int page) {
            _pageCurrent = page;
            _menuGroup.CloseCurrentThenOpen(_pages[_pageCurrent]);
            UpdateButtonStates();
        }

        public void PageNext() {
            if (_pageCurrent < _pageLimitCurrent) ChangePage(_pageCurrent + 1);
        }

        public void PagePrevious() {
            if (_pageCurrent > 0) ChangePage(_pageCurrent - 1);
        }

        private void UpdateButtonStates() {
            _pageForwardBtn.gameObject.SetActive(_pageCurrent < _pageLimitCurrent);
            _pageBackBtn.gameObject.SetActive(_pageCurrent > 0);
            if (EventSystem.current.currentSelectedGameObject && !EventSystem.current.currentSelectedGameObject.activeInHierarchy) {
                if (_pageForwardBtn.gameObject.activeInHierarchy) _pageForwardBtn.Select();
                else if (_pageBackBtn.gameObject.activeInHierarchy) _pageBackBtn.Select();
            }
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