using Ivayami.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.UI
{
    public class UseItemUIIcon : MonoBehaviour
    {
        [Header("References")]

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _textAmount;
        [SerializeField, Range(0f,1f)] private float _iconTransparency;

        public void SetItemDisplay(PlayerInventory.InventoryItemStack item)
        {
            bool isValid = item.Item;
            _icon.sprite = isValid ? item.Item.Sprite : null;
            _icon.color = isValid ? Color.white : new Color(1, 1, 1, _iconTransparency);
            _textAmount.text = isValid ? item.Amount.ToString() : null;
        }

        public void SetItemDisplay(InventoryItem item)
        {
            _icon.sprite = item.Sprite;
            _icon.color = new Color(1, 1, 1, _iconTransparency);
            _textAmount.text = "0";
        }
    }
}