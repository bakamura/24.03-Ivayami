using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;
using TMPro;

namespace Ivayami.UI {
    public class BagItem : MonoBehaviour {

        [Header("References")]

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _textAmount;

        [Header("Cache")]

        private PlayerInventory.InventoryItemStack item;

        public void SetItemDisplay(PlayerInventory.InventoryItemStack item) {
            this.item = item;
            bool isValid = item.Item;
            _icon.sprite = isValid ? item.Item.Sprite : null;
            _icon.color = isValid ? Color.white : new Color(0, 0, 0, 0);
            _textAmount.text = isValid ? item.Amount.ToString() : null;
        }

        public void DisplayInfo() {
            Bag.Instance.DisplayItemInfo(item.Item);
        }

    }
}
