using UnityEngine;
using UnityEngine.UI;
using Paranapiacaba.Player;

namespace Paranapiacaba.UI {
    public class Bag : MonoBehaviour {

        [SerializeField] private Button _itemBtnPrefab;
        [SerializeField] private Transform _itemSpecialSection;
        [SerializeField] private Transform _itemGeneralSection;
        [SerializeField] private Image _itemFocusPreview;

        public void InventoryUpdate() {
            InventoryItem[] items = PlayerInventory.Instance.CheckInventory();
            foreach (InventoryItem item in items) {
                Instantiate(null, item.type == ItemType.Special ? _itemSpecialSection : _itemGeneralSection);
            }
        }

        public void FocusItem(byte itemIdInBag) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}