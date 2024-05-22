using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryItem[] _itens;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private bool _isLongInteraction;

        private InteractableHighlight _interatctableHighlight;

        public InteractableHighlight InteratctableHighlight
        {
            get
            {
                if (!_interatctableHighlight)
                    _interatctableHighlight = GetComponent<InteractableHighlight>();
                return _interatctableHighlight;
            }
        }
        private InteractionPopup _interactionPopup;

        public InteractionPopup InteractionPopup
        {
            get
            {
                if (!_interactionPopup)
                    _interactionPopup = GetComponent<InteractionPopup>();
                return _interactionPopup;
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
            _onInteract?.Invoke();
        }
    }
}