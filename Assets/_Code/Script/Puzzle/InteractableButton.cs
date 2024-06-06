using UnityEngine;

namespace Ivayami.Puzzle
{    
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated))]
    public class InteractableButton : Activator, IInteractable
    {
        private InteractableFeedbacks _interatctableHighlight;
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
            IsActive = !IsActive;
            onActivate?.Invoke();
        }
    }
}