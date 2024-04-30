using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteratctableHighlight), typeof(ActivatorAnimated))]
    public class Lever : Activator, IInteractableLong
    {
        private InteratctableHighlight _interatctableHighlight;
        public InteratctableHighlight InteratctableHighlight => _interatctableHighlight;

        private void Awake()
        {
            _interatctableHighlight = GetComponent<InteratctableHighlight>();
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