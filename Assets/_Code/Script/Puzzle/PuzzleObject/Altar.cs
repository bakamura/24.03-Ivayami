using Ivayami.Audio;
using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
    public class Altar : MonoBehaviour, IInteractable
    {
        [SerializeField] private DeliverUI _deliverUI;
        [SerializeField] private GameObject _itemVisual;
        [SerializeField] private ItemData[] _itemsData;
        public InteractableFeedbacks InteratctableHighlight
        {
            get
            {
                if (!m_interatctableHighlight && this) m_interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return m_interatctableHighlight;
            }
        }
        private InteractableFeedbacks m_interatctableHighlight;
        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }
        }
        private InteractableSounds m_interactableSounds;
        private InventoryItem _currentItem;
        private MeshFilter _itemMeshFilter;
        private MeshRenderer _itemMeshRenderer;
        [System.Serializable]
        private struct ItemData
        {
            public InventoryItem Item;
            public Mesh ItemModel;
            public Material ItemMaterial;
        }

        public PlayerActions.InteractAnimation Interact()
        {
            if (_currentItem)
            {

            }
            else
            {
                _deliverUI.UpdateUI(true);
            }
            return PlayerActions.InteractAnimation.Default;
        }

        public void SetItemVisual(int index)
        {

        }
    }
}