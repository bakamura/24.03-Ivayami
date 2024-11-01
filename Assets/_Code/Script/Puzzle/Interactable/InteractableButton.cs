using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{    
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated))]
    public class InteractableButton : Activator, IInteractable
    {
        private InteractableFeedbacks _interatctableHighlight;
        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableHighlight && this) _interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return _interatctableHighlight;
            }
        }

        public PlayerActions.InteractAnimation Interact()
        {
            IsActive = !IsActive;
            onActivate?.Invoke();
            return PlayerActions.InteractAnimation.PushButton;
        }

        public void ForceInteract() => Interact();
    }
}