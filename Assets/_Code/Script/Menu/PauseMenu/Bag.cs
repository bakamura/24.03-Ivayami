using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Bag : MonoBehaviour {

        [SerializeField] private BagItem _itemBtnPrefab;
        [SerializeField] private Transform _itemSpecialSection;
        [SerializeField] private Transform _itemGeneralSection;
        [SerializeField] private Transform _itemFocusPreview;

        private List<BagItem> _itemBtns;

        public void InventoryUpdate() {
            InventoryItem[] items = PlayerInventory.Instance.CheckInventory();

            for (int i = 0; i < items.Length; i++) {
                if (_itemBtns.Count <= i) _itemBtns.Add(Instantiate(_itemBtnPrefab, _itemGeneralSection));
                _itemBtns[i].transform.parent = items[i].Type == ItemType.Special ? _itemSpecialSection : _itemGeneralSection;
                _itemBtns[i].SetItemDisplay(items[i]);
                byte btnN = (byte)i;
                _itemBtns[i].GetComponent<Button>().onClick.AddListener(() => FocusItem(btnN));
            }
        }

        public void FocusItem(byte itemIdInBag) {
            while (_itemFocusPreview.childCount > 0) Destroy(_itemFocusPreview.GetChild(0).gameObject);
            Instantiate(Resources.Load<GameObject>($"ItemMesh/{itemIdInBag}"), _itemFocusPreview); //
        }

    }
}