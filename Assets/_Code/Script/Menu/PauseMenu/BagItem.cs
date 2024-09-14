using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class BagItem : MonoBehaviour {

        [Header("References")]

        [SerializeField] private Image _icon;

        [Header("Cache")]

        private InventoryItem item;

        public void SetItemDisplay(InventoryItem item) {
            this.item = item;
            _icon.sprite = item != null ? item.Sprite : null;
            _icon.color = item != null ? Color.white : new Color(0, 0, 0, 0);
        }

        public void DisplayInfo() {
            Bag.Instance.DisplayItemInfo(item);
        }

    }
}
