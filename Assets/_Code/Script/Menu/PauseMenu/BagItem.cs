using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class BagItem : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField] private Image _itemPreview;

        [Header("Cache")]

        private Button _button;

        private void Awake() {
            _button = GetComponent<Button>();
        }

        public void SetItemDisplay(InventoryItem item) {
            name = item.DisplayName;
            if (_itemPreview != null) Destroy(_itemPreview);
            _itemPreview.sprite = item.Sprite;
            _itemPreview.transform.localPosition = Vector3.zero;
        }

    }
}
