using UnityEngine;
using System;
using System.Collections;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int WALKING_BOOL = Animator.StringToHash("walking");
        private static readonly int SPAWNING_TRIGGER = Animator.StringToHash("spawning");
        private static readonly int ATTACK_TRIGGER = Animator.StringToHash("attacking");
        private static readonly int TARGET_DETECTED_TRIGGER = Animator.StringToHash("targetDetected");
        private static readonly int INTERACT_TRIGGER = Animator.StringToHash("interacting");
        private static readonly int TAKE_DAMAGE_TRIGGER = Animator.StringToHash("takeDamage");

        private static readonly int WALKING_STATE = Animator.StringToHash("walk");
        private static readonly int SPAWNING_STATE = Animator.StringToHash("spawn");
        private static readonly int ATTACK_STATE = Animator.StringToHash("attack");
        private static readonly int TARGET_DETECTED_STATE = Animator.StringToHash("targetDetect");
        private static readonly int INTERACT_STATE = Animator.StringToHash("interact");
        private static readonly int TAKE_DAMAGE_STATE = Animator.StringToHash("damage");

        private Animator _animator
        {
            get
            {
                if (!m_animator) m_animator = GetComponent<Animator>();
                return m_animator;
            }
        }
        private Animator m_animator;
        private Coroutine _waitAnimationEndCoroutine;
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Walking(bool walking, Action onAnimationEnd = null)
        {
            _animator.SetBool(WALKING_BOOL, walking);
            StartAnimationEvent(WALKING_STATE, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Spawning(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(SPAWNING_TRIGGER);
            StartAnimationEvent(SPAWNING_STATE, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Attack(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(ATTACK_TRIGGER);
            StartAnimationEvent(ATTACK_STATE, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void TargetDetected(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(TARGET_DETECTED_TRIGGER);
            StartAnimationEvent(TARGET_DETECTED_STATE, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Interact(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(INTERACT_TRIGGER);
            StartAnimationEvent(INTERACT_STATE, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void TakeDamage(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(TAKE_DAMAGE_TRIGGER);
            StartAnimationEvent(TAKE_DAMAGE_STATE, onAnimationEnd);
        }

        private void StartAnimationEvent(int stateHash, Action onAnimationEnd)
        {
            if(onAnimationEnd != null)
            {
                if (_waitAnimationEndCoroutine != null)
                {
                    StopCoroutine(_waitAnimationEndCoroutine);
                    _waitAnimationEndCoroutine = null;
                }
                _waitAnimationEndCoroutine = StartCoroutine(WaitAnimationEndCoroutine(stateHash, onAnimationEnd));
            }
        }

        private IEnumerator WaitAnimationEndCoroutine(int stateHash, Action onAnimationEnd)
        {
            if(_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
            {
                while (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                {
                    yield return null;
                }
            }
            while (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash)
            {
                yield return null;
            }
            onAnimationEnd?.Invoke();
            _waitAnimationEndCoroutine = null;
        }
    }
}