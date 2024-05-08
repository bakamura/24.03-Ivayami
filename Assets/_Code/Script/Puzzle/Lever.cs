using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableHighlight), typeof(ActivatorAnimated))]
    public class Lever : Activator, IInteractableLong
    {
        private InteractableHighlight _interatctableHighlight;
        public InteractableHighlight InteratctableHighlight => _interatctableHighlight;

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