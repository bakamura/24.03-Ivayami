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
        public InventoryItem Item => item.Item;
        private Highlightable _highlightable;
        public Highlightable Highlightable { get { return _highlightable; } }

        private void Awake() {
            if(TryGetComponent(out _highlightable)) Debug.LogError($"Couldn't get {nameof(Highlightable)} from '{name}'");
        }

        public void SetItemDisplay(PlayerInventory.InventoryItemStack item) {
            this.item = item;
            bool isValid = item.Item;
            _icon.sprite = isValid ? item.Item.Sprite : null;
            _icon.color = isValid ? Color.white : new Color(0, 0, 0, 0);
            if (isValid && !item.Item.DisplayTextFormatedExternaly) _textAmount.text = item.Amount > 1 ? item.Amount.ToString() : null;
        }

        public void UpdateDisplayText(string text) {
            if (item.Item.DisplayTextFormatedExternaly) _textAmount.text = text;
        }

        public void DisplayInfo() {
            Bag.Instance.DisplayItemInfo(item.Item);
        }

    }
}
