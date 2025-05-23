using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks))]
    public sealed class RotatingPedestalPuzzleObject : MonoBehaviour
    {
        [SerializeField] private GameObject _itemInDisplay;
        private InteractableFeedbacks _interatctableFeedbacks;
        private bool _hasItem;

        public bool HasItem => _hasItem;

        [HideInInspector] public sbyte Index;
        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableFeedbacks) _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
                return _interatctableFeedbacks;
            }
        }

        public void UpdateItem(InventoryItem item, bool checkForItem = true)
        {
            if (!_hasItem && (PlayerInventory.Instance.CheckInventoryFor(item.name).Item || !checkForItem))
            {
                _itemInDisplay.SetActive(true);
                if (checkForItem) PlayerInventory.Instance.RemoveFromInventory(item);
                _hasItem = true;
            }
            else if (_hasItem)
            {
                _itemInDisplay.SetActive(false);
                if (checkForItem) PlayerInventory.Instance.AddToInventory(item);
                _hasItem = false;
            }
        }

        public bool IsCorrect(sbyte index)
        {
            return index == Index && _hasItem;
        }
    }
}