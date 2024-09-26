using UnityEngine;
using System;
using System.Collections;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        [SerializeField] private bool _animationScaleWithMovementSpeed;
        //private static readonly int WALKING_BOOL = Animator.StringToHash("walking");
        private static readonly int SPAWNING_TRIGGER = Animator.StringToHash("spawning");
        private static readonly int ATTACK_TRIGGER = Animator.StringToHash("attacking");
        private static readonly int TARGET_DETECTED_TRIGGER = Animator.StringToHash("targetDetected");
        private static readonly int INTERACT_TRIGGER = Animator.StringToHash("interacting");
        private static readonly int TAKE_DAMAGE_TRIGGER = Animator.StringToHash("takeDamage");
        private static readonly int CHASING_FLOAT = Animator.StringToHash("chasing");
        private static readonly int MOVE_SPEED_FLOAT = Animator.StringToHash("moveSpeed");

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
        public void Walking(float speed, Action onAnimationEnd = null)
        {
            //_animator.SetBool(WALKING_BOOL, walking);
            _animator.SetFloat(MOVE_SPEED_FLOAT, _animationScaleWithMovementSpeed ? speed : Math.Clamp(speed, 0, 1));
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
        public void Attack(Action onAnimationEnd = null, Action<float> currentAnimationStepCallback = null)
        {
            _animator.SetTrigger(ATTACK_TRIGGER);
            StartAnimationEvent(ATTACK_STATE, onAnimationEnd, currentAnimationStepCallback);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void TargetDetected(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(TARGET_DETECTED_TRIGGER);
            _animator.SetFloat(CHASING_FLOAT, 1);
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
        /// <param name="isChasing"></param>
        /// <param name="onAnimationEnd">will only activate once</param>
        public void Chasing(bool isChasing)
        {
            _animator.SetFloat(CHASING_FLOAT, isChasing ? 1 : 0);
        }

        private void StartAnimationEvent(int stateHash, Action onAnimationEnd, Action<float> currentAnimationStepCallback = null)
        {
            if (onAnimationEnd != null)
            {
                if (_waitAnimationEndCoroutine != null)
                {
                    StopCoroutine(_waitAnimationEndCoroutine);
                    _waitAnimationEndCoroutine = null;
                }
                _waitAnimationEndCoroutine = StartCoroutine(WaitAnimationEndCoroutine(stateHash, onAnimationEnd, currentAnimationStepCallback));
            }
        }

        private IEnumerator WaitAnimationEndCoroutine(int stateHash, Action onAnimationEnd, Action<float> currentAnimationStepCallback = null)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
            {
                while (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                {
                    yield return null;
                }
            }
            while (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash)
            {
                currentAnimationStepCallback?.Invoke(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                yield return null;
            }
            onAnimationEnd?.Invoke();
            _waitAnimationEndCoroutine = null;
        }
    }
}