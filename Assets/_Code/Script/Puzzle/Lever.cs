using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated))]
    public class Lever : Activator, IInteractableLong
    {
        private InteractableFeedbacks _interatctableHighlight;
        public InteractableFeedbacks InteratctableHighlight => _interatctableHighlight;

        private void Awake()
        {
            _interatctableHighlight = GetComponent<InteractableFeedbacks>();
        }

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