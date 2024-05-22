using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableHighlight), typeof(ActivatorAnimated))]
    public class Lever : Activator, IInteractableLong
    {
        private InteractableHighlight _interatctableHighlight;
        private InteractionPopup _interactionPopup;
        public InteractableHighlight InteratctableHighlight => _interatctableHighlight;

        public InteractionPopup InteractionPopup
        {
            get
            {
                if (!_interactionPopup)
                    _interactionPopup = GetComponent<InteractionPopup>();
                return _interactionPopup;
            }
        }

        private void Awake()
        {
            _interatctableHighlight = GetComponent<InteractableHighlight>();
        }

        public void Interact()
        {
            IsActive = true;
            onActivate?.Invoke();
        }

        public void InteractStop()
        {
            IsActive = false;
            onActivate?.Invoke();
        }
    }
}