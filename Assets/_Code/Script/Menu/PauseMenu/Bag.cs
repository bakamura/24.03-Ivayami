using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Paranapiacaba.Player;

namespace Paranapiacaba.UI {
    public class Bag : MonoBehaviour {

        [SerializeField] private BagItem _itemBtnPrefab;
        [SerializeField] private Transform _itemSpecialSection;
        [SerializeField] private Transform _itemGeneralSection;
        [SerializeField] private GameObject _itemFocusPreview;

        private List<BagItem> _itemBtns;

        public void InventoryUpdate() {
            InventoryItem[] items = PlayerInventory.Instance.CheckInventory();

            for (int i = 0; i < items.Length; i++) {
                if (_itemBtns.Count <= i) _itemBtns.Add(Instantiate(_itemBtnPrefab, _itemGeneralSection));
                _itemBtns[i].transform.parent = items[i].type == ItemType.Special ? _itemSpecialSection : _itemGeneralSection;
                _itemBtns[i].SetItemDisplay(items[i]);
                byte btnN = (byte)i;
                _itemBtns[i].GetComponent<Button>().onClick.AddListener(() => FocusItem(btnN));
            }
        }

        public void FocusItem(byte itemIdInBag) {
            _itemFocusPreview = Instantiate(_itemFocusPreview); //
        }

    }
}