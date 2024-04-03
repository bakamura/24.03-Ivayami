using UnityEngine;
using System.Linq;

namespace Paranapiacaba.UI {
    public class Animated : Menu {

        [Header("Cache")]

        private Animator _animator;
        private static int _openId = Animator.StringToHash("Open");
        private static int _closeId = Animator.StringToHash("Close");

        private void Awake() {
            _animator = GetComponent<Animator>();
            _transitionDuration = _animator.runtimeAnimatorController.animationClips.FirstOrDefault(x => x.name == "Open").length;
        }

        public override void Open() {
            _animator.SetTrigger(_openId);
        }

        public override void Close() {
            _animator.SetTrigger(_closeId);
        }

    }
}