using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Bag : MonoSingleton<Bag> {

        [Header("Parameters")]

        [SerializeField] private BagItem[] _itemNormalBtns;
        [SerializeField] private BagItem[] _itemSpecialBtns;
        [SerializeField] private TextMeshProUGUI _itemDescriptor;

        [Header("Quick Open")]

        [SerializeField] private InputActionReference _quickOpenInput;
        [SerializeField] private Button _quickOpenBtn;

        private Dictionary<int, BagItem> _itemBtnsByCurrentItem = new Dictionary<int, BagItem>();

        private void Start() {
            _quickOpenInput.action.performed += QuickOpen;
            PlayerInventory.Instance.onItemStackUpdate += HandleItemRemoved;
        }       

        public void InventoryUpdate() {
            PlayerInventory.InventoryItemStack[] items = PlayerInventory.Instance.CheckInventory();
            Queue<PlayerInventory.InventoryItemStack> normalQ = new Queue<PlayerInventory.InventoryItemStack>();
            Queue<PlayerInventory.InventoryItemStack> specialQ = new Queue<PlayerInventory.InventoryItemStack>();
            foreach (PlayerInventory.InventoryItemStack item in items) {
                if (item.Item.Type == ItemType.Special) specialQ.Enqueue(item);
                else if(item.Item.Type != ItemType.Document) normalQ.Enqueue(item);
            }
            for (int i = 0; i < _itemNormalBtns.Length; i++) _itemNormalBtns[i].SetItemDisplay(normalQ.Count > 0 ? normalQ.Dequeue() : new PlayerInventory.InventoryItemStack());
            for (int i = 0; i < _itemSpecialBtns.Length; i++) _itemSpecialBtns[i].SetItemDisplay(specialQ.Count > 0 ? specialQ.Dequeue() : new PlayerInventory.InventoryItemStack());

        }

        public void DisplayItemInfo(InventoryItem item) {
            _itemDescriptor.text = item ? $"{item.GetDisplayName()}\n{item.GetDisplayDescription()}" : "";
        }

        public void UpdateItemDisplayText(InventoryItem item, string text)
        {
            //perhaps because of the lantern metter, the bag UI will need to have its own canvas for optimize draw call
            if (!PlayerInventory.Instance.CheckInventoryFor(item.name).Item) return;
            if (!_itemBtnsByCurrentItem.ContainsKey(item.GetInstanceID()))
            {
                BagItem[] allItems = _itemNormalBtns.Union(_itemSpecialBtns).ToArray();
                for(int i = 0; i < allItems.Length; i++)
                {
                    if (allItems[i].Item == item && allItems[i].Item.DisplayTextFormatedExternaly)
                    {
                        _itemBtnsByCurrentItem.Add(item.GetInstanceID(), allItems[i]);
                        break;
                    }
                }
            }
            _itemBtnsByCurrentItem[item.GetInstanceID()].UpdateDisplayText(text);
        }

        private void QuickOpen(InputAction.CallbackContext context) {
            if (!Pause.Instance.Paused) {
                Pause.Instance.PauseGame(true);
                _quickOpenBtn.onClick.Invoke();
            }
        }

        private void HandleItemRemoved(PlayerInventory.InventoryItemStack itemStack)
        {
            if (_itemBtnsByCurrentItem.ContainsKey(itemStack.Item.GetInstanceID()))
            {
                _itemBtnsByCurrentItem.Remove(itemStack.Item.GetInstanceID());
            }
        }

    }
}