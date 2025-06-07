using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using Ivayami.Audio;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableSounds))]
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField, Tooltip("The items that will be given to the player on Interact")] private InventoryItem[] _itens;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private bool _isLongInteraction;
        [SerializeField] private bool _playItemCollectedFeedbacks = true;
        [SerializeField] private bool _emphasizeCollect;

        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }
        }
        private InteractableSounds m_interactableSounds;

        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!m_interatctableHighlight && this) m_interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return m_interatctableHighlight;
            }
        }
        private InteractableFeedbacks m_interatctableHighlight;

        public PlayerActions.InteractAnimation Interact()
        {
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            GiveItem();
            return PlayerActions.InteractAnimation.Default;
        }

        public void ForceInteract() => Interact();

        public void GiveItem()
        {
            for (int i = 0; i < _itens.Length; i++)
            {
                PlayerInventory.Instance.AddToInventory(_itens[i], _emphasizeCollect, _playItemCollectedFeedbacks);
            }
            _onInteract?.Invoke();
        }
    }
}