using UnityEngine;
using Paranapiacaba.Player;
using UnityEngine.Events;

namespace Paranapiacaba.Puzzle
{
    public class ReciveItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryItem[] _itens;
        [SerializeField] private UnityEvent _onCollect;
        [SerializeField] private bool _isLongInteraction;

        public bool Interact()
        {
            GiveItem();
            return _isLongInteraction;
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