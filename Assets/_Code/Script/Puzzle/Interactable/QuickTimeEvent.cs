using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;
using Ivayami.Player;
using Ivayami.Audio;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
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
        private InteractableFeedbacks m_interactableFeedbacks;
        private InteractableSounds m_interactableSounds;
        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }
        }
        private InteractableFeedbacks _interactableFeedbacks
        {
            get
            {
                if (!m_interactableFeedbacks) m_interactableFeedbacks = GetComponent<InteractableFeedbacks>();
                return m_interactableFeedbacks;
            }
        }
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

        private const string BLOCK_KEY = "QuickTimeEvent";

        public void BeginEvent()
        {
            if (_waitAnimationCoroutine == null)
            {
                _quickTimeButton.action.started += HandleOnClick;
                PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false);
                _onStart?.Invoke();
                for (int i = 0; i < _animations.Length; i++)
                {
                    _animations[i].Record();
                    _animations[i].Animator.Play(_animations[i].StateHash, 0);
                }
                _interactableFeedbacks.UpdateFeedbacks(false, true);
                _waitAnimationCoroutine = StartCoroutine(WaitAnimationEndCoroutine(true, () => {
                    SetAllSpeeds(false);
                    _interactableFeedbacks.UpdateFeedbacks(true, true);
                }));
            }
        }

        public void EndEvent()
        {
            _quickTimeButton.action.started -= HandleOnClick;
            PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, true);
            _currentClickAmount = 0;
            SetAllSpeeds(true);
            _onEnd?.Invoke();
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
                _interactableFeedbacks.PlayInteractionAnimation();
                _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
                if (_currentClickAmount >= _amountOfTimesToClick)
                {
                    _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionSuccess);
                    SetAllSpeeds(true);
                    _interactableFeedbacks.UpdateFeedbacks(false, true);
                    StartCoroutine(WaitAnimationEndCoroutine(false, EndEvent));
                }
                else
                {
                    AnimationClip temp;
                    for (int i = 0; i < _animations.Length; i++)
                    {
                        temp = _animations[i].Animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                        _animations[i].Animator.Play(_animations[i].StateHash, 0, _currentClickAmount * (_framesAdvancePerClick / (temp.frameRate * temp.length)) + .01f);
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
            _waitAnimationCoroutine = null;
            onWaitEnd?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _amountOfTimesToClick = System.Math.Clamp(_amountOfTimesToClick, byte.MinValue, byte.MaxValue);
            _framesAdvancePerClick = System.Math.Clamp(_framesAdvancePerClick, byte.MinValue, byte.MaxValue);
        }
#endif
    }
}