using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class ReciveItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryItem[] _itens;
        [SerializeField] private UnityEvent _onCollect;
        [SerializeField] private bool _isLongInteraction;

        private InteratctableHighlight _interatctableHighlight;

        public InteratctableHighlight InteratctableHighlight
        {
            get
            {
                if (!_interatctableHighlight)
                    _interatctableHighlight = GetComponent<InteratctableHighlight>();
                return _interatctableHighlight;
            }
        }

        public void Interact()
        {
            GiveItem();
        }

        public void GiveItem()
        {
            for (int i = 0; i < _itens.Length; i++)
            {
                PlayerInventory.Instance.AddToInventory(_itens[i]);
            }
            _onCollect?.Invoke();
        }
    }
}