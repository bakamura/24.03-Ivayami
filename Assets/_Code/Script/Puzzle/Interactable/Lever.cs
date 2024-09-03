using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated))]
    public class Lever : Activator, IInteractableLong
    {
        [SerializeField] private PlayerActions.InteractAnimation _animationType;
        public InteractableFeedbacks InteratctableHighlight
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
            IsActive = true;
            onActivate?.Invoke();
            return _animationType;
        }

        public void InteractStop()
        {
            IsActive = false;
            onActivate?.Invoke();
        }
    }
}