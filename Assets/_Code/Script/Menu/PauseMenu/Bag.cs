using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Bag : MonoSingleton<Bag> {

        [Header("Parameters")]

        [SerializeField] private BagItem[] _itemNormalBtns;
        [SerializeField] private BagItem[] _itemSpecialBtns;
        [SerializeField] private TextMeshProUGUI _itemDescriptor;

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

    }
}