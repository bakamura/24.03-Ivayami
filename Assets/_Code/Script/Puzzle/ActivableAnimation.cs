using UnityEngine;

namespace Paranapiacaba.Puzzle
{
    [RequireComponent(typeof(Animator))]
    public class ActivableAnimation : Activable, IInteractable
    {
        [SerializeField] private bool _animateByInteraction;
        [SerializeField] private string _animationNameToPlay;

        private Animator _animator;

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
        }

        public void Interact()
        {
            if (IsActive && _animateByInteraction) TriggerAnimation();
        }

        public void TriggerAnimation()
        {
            if (IsActive)
            {
                if (!string.IsNullOrEmpty(_animationNameToPlay)) _animator.Play(_animationNameToPlay);
                else _animator.SetBool("activate", !_animator.GetBool("activate"));
            }
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            TriggerAnimation();
        }
    }
}