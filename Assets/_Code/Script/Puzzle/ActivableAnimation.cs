using UnityEngine;

namespace Paranapiacaba.Puzzle
{
    [RequireComponent(typeof(Animator))]
    public class ActivableAnimation : Activable, IInteractable
    {
        private const string _animationToPlayOnInteract = "interact";
        private const string _animationToPlayOnActivate = "activate";

        private Animator _animator;

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
        }

        public bool Interact()
        {
            if (IsActive) TriggerAnimation(_animationToPlayOnInteract);
            return false;
        }

        public void TriggerAnimation(string boolName)
        {
            _animator.SetBool(boolName, !_animator.GetBool(boolName));
        }
        
        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            TriggerAnimation(_animationToPlayOnActivate);
        }
    }
}