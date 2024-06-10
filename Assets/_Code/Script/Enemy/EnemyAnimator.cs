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
        private static readonly int ATTACK = Animator.StringToHash("attack");

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
            StartAnimationEvent(WALKING, onAnimationEnd);
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
            StartAnimationEvent(SPAWNING, onAnimationEnd);
        }

        public void Attack(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(ATTACK);
            StartAnimationEvent(ATTACK, onAnimationEnd);
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
            if (!_animator.GetCurrentAnimatorStateInfo(0).loop)
            {
                while (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                    yield return null;
            }
            else
            {
                float count = 0;
                while (_animator.GetCurrentAnimatorStateInfo(0).length > count)
                {
                    count += Time.deltaTime;
                    yield return null;
                }
            }
            onAnimationEnd?.Invoke();
            _waitAnimationEndCoroutine = null;
        }
    }
}