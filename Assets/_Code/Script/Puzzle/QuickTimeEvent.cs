using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    public class QuickTimeEvent : MonoBehaviour
    {
        [SerializeField] private InputActionReference _quickTimeButton;
        [SerializeField] private byte _amountOfTimesToClick;
        [SerializeField] private byte _framesAdvancePerClick;
        [SerializeField] private UnityEvent _onStart;
        [SerializeField] private UnityEvent _onEnd;
        [SerializeField] private AnimatorInfo[] _animations;

        private byte _currentClickAmount;
        private Coroutine _waitAnimationCoroutine;
        [System.Serializable]
        private struct AnimatorInfo
        {
            public Animator Animator;
            public string StateToGo;
            [HideInInspector] public int StateHash;
            [HideInInspector] public float AnimatorSpeed;

            public void Record()
            {
                StateHash = Animator.StringToHash(StateToGo);
                AnimatorSpeed = Animator.speed;
            }

            public void ChangeSpeed(bool setRecorderSpeed)
            {
                Animator.speed = setRecorderSpeed ? AnimatorSpeed : 0;
            }
        }

        public void BeginEvent()
        {
            if (_waitAnimationCoroutine == null)
            {
                _quickTimeButton.action.started += HandleOnClick;
                PlayerMovement.Instance.ToggleMovement(false);
                _onStart?.Invoke();
                for (int i = 0; i < _animations.Length; i++)
                {
                    _animations[i].Record();
                    _animations[i].Animator.Play(_animations[i].StateHash, 0);
                }
                _waitAnimationCoroutine = StartCoroutine(WaitAnimationEndCoroutine(true, () => SetAllSpeeds(false)));
            }
        }

        public void EndEvent()
        {
            _quickTimeButton.action.started -= HandleOnClick;
            PlayerMovement.Instance.ToggleMovement(true);
            _currentClickAmount = 0;
            _onEnd?.Invoke();
            SetAllSpeeds(true);
        }

        private void SetAllSpeeds(bool setRecorderSpeed)
        {
            for (int i = 0; i < _animations.Length; i++)
            {
                _animations[i].ChangeSpeed(setRecorderSpeed);
            }
        }

        private void HandleOnClick(InputAction.CallbackContext ctx)
        {
            if (_waitAnimationCoroutine == null)
            {
                _currentClickAmount++;
                if (_currentClickAmount > _amountOfTimesToClick)
                {
                    SetAllSpeeds(true);
                    StartCoroutine(WaitAnimationEndCoroutine(false, EndEvent));
                }
                else
                {
                    AnimationClip temp;
                    for (int i = 0; i < _animations.Length; i++)
                    {
                        temp = _animations[i].Animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                        _animations[i].Animator.Play(_animations[i].StateHash, 0, _currentClickAmount * (_framesAdvancePerClick / (temp.length * temp.frameRate)));                        
                    }
                }
            }
        }

        private IEnumerator WaitAnimationEndCoroutine(bool waitToEnterState, Action onWaitEnd)
        {
            byte count = 0;
            while (count < _animations.Length)
            {
                count = 0;
                for (int i = 0; i < _animations.Length; i++)
                {
                    if (waitToEnterState)
                    {
                        if (_animations[i].Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == _animations[i].StateHash) count++;
                    }
                    else
                    {
                        if (_animations[i].Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _animations[i].StateHash) count++;
                    }
                }
                yield return null;
            }
            onWaitEnd?.Invoke();
            _waitAnimationCoroutine = null;
        }

        private void OnValidate()
        {
            _amountOfTimesToClick = System.Math.Clamp(_amountOfTimesToClick, byte.MinValue, byte.MaxValue);
            _framesAdvancePerClick = System.Math.Clamp(_framesAdvancePerClick, byte.MinValue, byte.MaxValue);
        }
    }
}