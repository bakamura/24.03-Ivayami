using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;
using Ivayami.Audio;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableSounds))]
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryItem[] _itens;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private bool _isLongInteraction;

        private InteractableFeedbacks _interatctableHighlight;
        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }
        }
        private InteractableSounds m_interactableSounds;

        public InteractableFeedbacks InteratctableHighlight
        {
            get
            {
                if (!_interatctableHighlight) _interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return _interatctableHighlight;
            }
        }

        public void Interact()
        {
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Collect);
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