using UnityEngine;

namespace Ivayami.UI {
    public class Animated : Menu {

        [Header("Animated")]

        [SerializeField] private AnimationClip _openAnimation;
        [SerializeField] private AnimationClip _closeAnimation;

        [Header("Cache")]

        private Animator _animator;
        private static int _openId = Animator.StringToHash("Open");
        private static int _closeId = Animator.StringToHash("Close");
        // Somehow hide in the inspector (Maybe custom inspector) TransitionDuration & AnimationCurve

        protected override void Awake() {
            base.Awake();

            _animator = GetComponent<Animator>();

            OnOpenStart.AddListener(() => {
                _animator.SetTrigger(_openId);
                _transitionDuration = _openAnimation.length;
            });
            OnCloseStart.AddListener(() => {
                _animator.SetTrigger(_closeId);
                _transitionDuration = _closeAnimation.length;
            });

        }

    }
}