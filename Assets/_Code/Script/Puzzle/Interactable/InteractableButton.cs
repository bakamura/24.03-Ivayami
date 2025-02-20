using Ivayami.Audio;
using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle
{    
    [RequireComponent(typeof(InteractableFeedbacks), typeof(ActivatorAnimated))]
    public class InteractableButton : Activator, IInteractable
    {
        [SerializeField] private PlayerActions.InteractAnimation _animationType;
        [SerializeField] private bool _startActive;
        [SerializeField] private bool _playAudioOnStart;

        private InteractableFeedbacks _interatctableHighlight;
        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableHighlight && this) _interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return _interatctableHighlight;
            }
        }

        private void Start()
        {
            if (_startActive)
            {
                InteractableSounds sounds = GetComponent<InteractableSounds>();
                if (!_playAudioOnStart) sounds.UpdateActiveState(false);
                Interact();
                sounds.UpdateActiveState(true);
            }
        }

        public PlayerActions.InteractAnimation Interact()
        {
            IsActive = !IsActive;
            onActivate?.Invoke();
            return _animationType;
        }

        public void ForceInteract() => Interact();
    }
}