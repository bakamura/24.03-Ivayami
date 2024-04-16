using UnityEngine;

namespace Paranapiacaba.Puzzle
{
    [RequireComponent(typeof(Animator))]
    public class ActivableAnimation : Activable, IInteractable
    {
        private static int _animationToPlayOnInteract = Animator.StringToHash("interact");
        private static int _animationToPlayOnActivate = Animator.StringToHash("activate");

        private Animator _animator;
        private InteratctableHighlight _interatctableHighlight;

        public InteratctableHighlight InteratctableHighlight { get => _interatctableHighlight; }

        protected override void Awake()
        {
            base.Awake();
            _interatctableHighlight = GetComponent<InteratctableHighlight>();
            _animator = GetComponent<Animator>();
        }

        public bool Interact()
        {
            if (IsActive) TriggerAnimation(_animationToPlayOnInteract);
            return false;
        }

        public void TriggerAnimation(int hash)
        {
            _animator.SetBool(hash, !_animator.GetBool(hash));
        }
        
        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            TriggerAnimation(_animationToPlayOnActivate);
        }
    }
}