using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.UI {
    public class Bag : MonoSingleton<Bag> {

        [Header("Parameters")]

        [SerializeField] private BagItem[] _itemNormalBtns;
        [SerializeField] private BagItem[] _itemSpecialBtns;
        [SerializeField] private TextMeshProUGUI _itemDescriptor;

        private List<BagItem> _itemBtns = new List<BagItem>();

        public void InventoryUpdate() {
            InventoryItem[] items = PlayerInventory.Instance.CheckInventory();
            Queue<InventoryItem> normalQ = new Queue<InventoryItem>();
            Queue<InventoryItem> specialQ = new Queue<InventoryItem>();
            foreach (InventoryItem item in items) {
                if (item.Type == ItemType.Special) specialQ.Enqueue(item);
                else normalQ.Enqueue(item);
            }
            for (int i = 0; i < _itemNormalBtns.Length; i++) _itemNormalBtns[i].SetItemDisplay(normalQ.Count > 0 ? normalQ.Dequeue() : null);
            for (int i = 0; i < _itemSpecialBtns.Length; i++) _itemSpecialBtns[i].SetItemDisplay(specialQ.Count > 0 ? specialQ.Dequeue() : null);

        }

        public void DisplayItemInfo(InventoryItem item) {
            InventoryItem itemTranslation = item?.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language);
            _itemDescriptor.text = itemTranslation != null ? $"{itemTranslation.DisplayName}\n{itemTranslation.Description}" : "";
        }

    }
}