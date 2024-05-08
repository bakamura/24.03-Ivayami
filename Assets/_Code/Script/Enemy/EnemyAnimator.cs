using UnityEngine;
using System;
using System.Collections;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int WALKING = Animator.StringToHash("walking");
        private static readonly int SPAWNING = Animator.StringToHash("spawning");

        private Animator _animator { get 
            {
                if (!_animatorCache) _animatorCache = GetComponent<Animator>();
                return _animatorCache;
            } }
        private Animator _animatorCache;
        private Action _onAnimationEnd;
        private Coroutine _waitAnimationEndCoroutine;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walking"></param>
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Walking(bool walking, Action onAnimationEnd = null)
        {
            _animator.SetBool(WALKING, walking);
            _onAnimationEnd = onAnimationEnd;
            StartAnimationEvent(WALKING);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Spawning(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(SPAWNING);
            _onAnimationEnd = onAnimationEnd;
            StartAnimationEvent(SPAWNING);
        }

        private void StartAnimationEvent(int stateHash)
        {
            if (_waitAnimationEndCoroutine != null) 
            {
                StopCoroutine(_waitAnimationEndCoroutine);
                _waitAnimationEndCoroutine = null;
            }
            _waitAnimationEndCoroutine = StartCoroutine(WaitAnimationEndCoroutine(stateHash));
        }

        private IEnumerator WaitAnimationEndCoroutine(int stateHash)
        {
            while (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                yield return null;
            _onAnimationEnd?.Invoke();
            _onAnimationEnd = null;
        }
    }
}