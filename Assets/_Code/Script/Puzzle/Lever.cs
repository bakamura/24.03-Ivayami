using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated))]
    public class Lever : Activator, IInteractableLong
    {
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
            return PlayerActions.InteractAnimation.Default;
        }

        public void InteractStop()
        {
            IsActive = false;
            onActivate?.Invoke();
        }
    }
}