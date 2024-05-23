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