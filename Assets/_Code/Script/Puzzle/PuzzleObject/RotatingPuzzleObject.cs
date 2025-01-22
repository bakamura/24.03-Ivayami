using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks))]
    public sealed class RotatingPuzzleObject : MonoBehaviour
    {
        [SerializeField] private GameObject _itemInDisplay;
        private InteractableFeedbacks _interatctableFeedbacks;
        private bool _hasItem;

        public byte Index { get; set; }
        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableFeedbacks) _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
                return _interatctableFeedbacks;
            }
        }

        public void UpdateItem(InventoryItem item)
        {
            if (!_hasItem && PlayerInventory.Instance.CheckInventoryFor(item.name).Item)
            {
                _itemInDisplay.SetActive(true);
                PlayerInventory.Instance.RemoveFromInventory(item);
                _hasItem = true;
            }
            else if (_hasItem)
            {
                _itemInDisplay.SetActive(false);
                PlayerInventory.Instance.AddToInventory(item);
                _hasItem = false;
            }
        }

        public bool IsCorrect(byte index)
        {
            return index == Index && _hasItem;
        }
    }
}