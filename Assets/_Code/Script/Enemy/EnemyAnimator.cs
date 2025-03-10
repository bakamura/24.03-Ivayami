using UnityEngine;
using System;
using System.Collections;
using Ivayami.Audio;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        [SerializeField] private bool _animationScaleWithMovementSpeed;
        [SerializeField, Min(0)] private int _attackAnimationLayer;
        //private static readonly int WALKING_BOOL = Animator.StringToHash("walking");
        private static readonly int SPAWNING_TRIGGER = Animator.StringToHash("spawning");
        private static readonly int ATTACK_TRIGGER = Animator.StringToHash("attacking");
        private static readonly int TARGET_DETECTED_TRIGGER = Animator.StringToHash("targetDetected");
        private static readonly int INTERACT_TRIGGER = Animator.StringToHash("interacting");
        private static readonly int PARALISE_BOOL = Animator.StringToHash("paralised");
        private static readonly int CHASING_BOOL = Animator.StringToHash("chasing");
        private static readonly int MOVE_SPEED_FLOAT = Animator.StringToHash("moveSpeed");
        private static readonly int ATTACK_INDEX_FLOAT = Animator.StringToHash("attackIndex");
        private static readonly int PARALISE_INDEX_FLOAT = Animator.StringToHash("paralisedIndex");

        private static readonly int WALKING_STATE = Animator.StringToHash("walk");
        private static readonly int CHASE_STATE = Animator.StringToHash("chase");
        private static readonly int SPAWNING_STATE = Animator.StringToHash("spawn");
        private static readonly int ATTACK_STATE = Animator.StringToHash("attack");
        private static readonly int TARGET_DETECTED_STATE = Animator.StringToHash("targetDetect");
        private static readonly int INTERACT_STATE = Animator.StringToHash("interact");
        private static readonly int PARALISE_STATE = Animator.StringToHash("paralise");

        private Animator _animator
        {
            get
            {
                if (!m_animator) m_animator = GetComponent<Animator>();
                return m_animator;
            }
        }
        private Animator m_animator;
        private EnemySounds _enemySound
        {
            get
            {
                if (!m_enemySound) m_enemySound = GetComponentInParent<EnemySounds>();
                return m_enemySound;
            }
        }
        private EnemySounds m_enemySound;
        private Coroutine _waitAnimationEndCoroutine;
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Walking(float speed, Action onAnimationEnd = null)
        {
            //_animator.SetBool(WALKING_BOOL, walking);
            _animator.SetFloat(MOVE_SPEED_FLOAT, _animationScaleWithMovementSpeed ? speed : Math.Clamp(speed, 0, 1));
            if(_animator.GetBool(CHASING_BOOL))StartAnimationEvent(CHASE_STATE, 0, onAnimationEnd);
            else StartAnimationEvent(WALKING_STATE, 0, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Spawning(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(SPAWNING_TRIGGER);
            StartAnimationEvent(SPAWNING_STATE, 0, onAnimationEnd);
        }
        /// <summary>
        /// will only activate once
        /// </summary>
        /// <param name="onAnimationEnd"></param>
        /// <param name="currentAnimationStepCallback"></param>
        /// <param name="attackAnimationIndex">Wich animation the enemy will play in the attack pool</param>
        public void Attack(Action onAnimationEnd = null, Action<float> currentAnimationStepCallback = null, int attackAnimationIndex = 0)
        {
            _animator.SetFloat(ATTACK_INDEX_FLOAT, attackAnimationIndex);
            _animator.SetTrigger(ATTACK_TRIGGER);
            StartAnimationEvent(ATTACK_STATE, _attackAnimationLayer, onAnimationEnd, currentAnimationStepCallback);
        }

        public int GetCurrentAttackAnimIndex()
        {
            return (int)_animator.GetFloat(ATTACK_INDEX_FLOAT);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void TargetDetected(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(TARGET_DETECTED_TRIGGER);
            _animator.SetBool(CHASING_BOOL, true);
            StartAnimationEvent(TARGET_DETECTED_STATE, 0, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Interact(Action onAnimationEnd = null)
        {
            _animator.SetTrigger(INTERACT_TRIGGER);
            StartAnimationEvent(INTERACT_STATE, 0, onAnimationEnd);
        }
        /// <param name="onAnimationEnd">
        /// will only activate once
        /// </param>
        public void Paralise(bool paralised, Action onAnimationEnd = null, int paraliseAnimationIndex = 0)
        {
            _animator.SetFloat(PARALISE_INDEX_FLOAT, paraliseAnimationIndex);
            _animator.SetBool(PARALISE_BOOL, paralised);
            StartAnimationEvent(PARALISE_STATE, 0, onAnimationEnd);
        }
        /// <param name="isChasing"></param>
        /// <param name="onAnimationEnd">will only activate once</param>
        public void Chasing(bool isChasing)
        {
            _animator.SetBool(CHASING_BOOL, isChasing);
        }

        public void PlayStepSound()
        {
            _enemySound.PlaySound(EnemySounds.SoundTypes.Steps);
        }

        private void StartAnimationEvent(int stateHash, int layer, Action onAnimationEnd, Action<float> currentAnimationStepCallback = null)
        {
            if (onAnimationEnd != null)
            {
                if (_waitAnimationEndCoroutine != null)
                {
                    StopCoroutine(_waitAnimationEndCoroutine);
                    _waitAnimationEndCoroutine = null;
                }
                _waitAnimationEndCoroutine = StartCoroutine(WaitAnimationEndCoroutine(stateHash, layer, onAnimationEnd, currentAnimationStepCallback));
            }
        }

        private IEnumerator WaitAnimationEndCoroutine(int stateHash, int layer, Action onAnimationEnd, Action<float> currentAnimationStepCallback = null)
        {
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash != stateHash)
            {
                while (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash != stateHash)
                {
                    yield return null;
                }
            }
            while (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == stateHash)
            {
                currentAnimationStepCallback?.Invoke(_animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
                yield return null;
            }
            onAnimationEnd?.Invoke();
            _waitAnimationEndCoroutine = null;
        }
    }
}