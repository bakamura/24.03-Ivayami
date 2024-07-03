using Ivayami.Audio;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
    public class ActivableAnimation : Activable, IInteractable
    {
        //[SerializeField] private bool _startActive;
        //[Header("PARAMETERS")]
        [SerializeField] private bool _lockOnActiveState;
        [SerializeField] private Animator _interactionAnimator;
        [SerializeField] private Animator _activateAnimator;
        //[SerializeField] private UnityEvent _onInteractStart;
        [Header("EVENTS")]
        [SerializeField] private AnimationEvent _onActivate;
        [SerializeField] private AnimationEvent _onDeactivate;
        [SerializeField] private AnimationEvent _onInteract;
        [SerializeField] private AnimationEvent _onInteractReturn;

        private static readonly int _interactBoolHash = Animator.StringToHash("interact");
        private static readonly int _activateBoolHash = Animator.StringToHash("activate");

        private static readonly int _interactionStateHash = Animator.StringToHash("interaction");
        private static readonly int _activationStateHash = Animator.StringToHash("activation");

        private InteractableFeedbacks _interatctableHighlight;
        private Coroutine _callbackCoroutine;
        private InteractableSounds _interactableSounds;
        private bool _activatedOnce;

        [System.Serializable]
        private class AnimationEvent
        {
            [Min(0f)] public float Delay;
            public bool DoOnce;
            public UnityEvent OnComplete;
            public bool EventTriggered { get; private set; }

            public void Setup()
            {
                OnComplete.AddListener(HandleEventTrigger);
            }

            private void HandleEventTrigger()
            {
                EventTriggered = true;
            }
        }

        public InteractableFeedbacks InteratctableHighlight { get => _interatctableHighlight; }

        protected override void Awake()
        {
            base.Awake();
            base.HandleOnActivate();
            if (_interactionAnimator) _interactionAnimator.SetBool(_activateBoolHash, IsActive);
            if (_activateAnimator) _activateAnimator.SetBool(_activateBoolHash, IsActive);
            _interatctableHighlight = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _onActivate.Setup();
            _onInteract.Setup();
        }

        private void OnDisable()
        {
            StopCallbackCoroutine();
        }

        public void Interact()
        {
            if (IsActive)
            {
                //_onInteractStart?.Invoke();
                if (_interactionAnimator)
                {
                    _interactionAnimator.SetBool(_interactBoolHash, !_interactionAnimator.GetBool(_interactBoolHash));
                    _interactableSounds.PlaySound(_interactionAnimator.GetBool(_interactBoolHash) ? InteractableSounds.SoundTypes.Interact : InteractableSounds.SoundTypes.InteractReturn);
                }
                CheckCallbacks(_interactBoolHash);
            }
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (_lockOnActiveState && _activatedOnce) return;
            if (IsActive) _activatedOnce = true;
            if (_interactionAnimator)
            {
                if (!IsActive) _interactionAnimator.SetBool(_interactBoolHash, false);
                _interactionAnimator.SetBool(_activateBoolHash, IsActive);
            }
            if (_activateAnimator)
            {
                _activateAnimator.SetBool(_activateBoolHash, IsActive);
                _interactableSounds.PlaySound(IsActive ? InteractableSounds.SoundTypes.Activate : InteractableSounds.SoundTypes.Deactivate);
            }
            CheckCallbacks(_activateBoolHash);
        }

        private IEnumerator CallbackDelayCoroutine(Animator animator, int stateHash, float delay, UnityEvent unityEvent)
        {
            if (animator)
            {
                // if is in other state wait for the state to finish
                if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                {
                    while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                    {
                        yield return null;
                    }
                }
                // wait for the stateHash to finish
                float count = 0;
                while (count < animator.GetCurrentAnimatorClipInfo(0)[0].clip.length)
                {
                    count += Time.deltaTime;
                    yield return null;
                }
            }
            if (delay > 0) yield return new WaitForSeconds(delay);
            unityEvent?.Invoke();
        }

        private void CheckCallbacks(int parameterHash)
        {
            StopCallbackCoroutine();
            if (parameterHash == _activateBoolHash && _activateAnimator)
            {
                if (_activateAnimator.GetBool(_activateBoolHash))
                {
                    if ((!_onActivate.DoOnce || _onActivate.DoOnce && !_onActivate.EventTriggered)
                        && _onActivate.OnComplete.GetPersistentEventCount() > 0)
                    {
                        _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_activateAnimator, _activationStateHash,
                            _onActivate.Delay, _onActivate.OnComplete));
                    }
                }
                else
                {
                    if ((!_onDeactivate.DoOnce || _onDeactivate.DoOnce && !_onDeactivate.EventTriggered)
                        && _onDeactivate.OnComplete.GetPersistentEventCount() > 0)
                    {
                        _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_activateAnimator, _activationStateHash,
                            _onDeactivate.Delay, _onDeactivate.OnComplete));
                    }
                }
            }
            else if (parameterHash == _interactBoolHash && _interactionAnimator && _interactionAnimator.GetBool(_activateBoolHash))
            {
                if (_interactionAnimator.GetBool(_interactBoolHash))
                {
                    if ((!_onInteract.DoOnce || _onInteract.DoOnce && !_onInteract.EventTriggered)
                        && _onInteract.OnComplete.GetPersistentEventCount() > 0)
                    {
                        _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_interactionAnimator, _interactionStateHash,
                            _onInteract.Delay, _onInteract.OnComplete));
                    }
                }
                else
                {
                    if ((!_onInteractReturn.DoOnce || _onInteractReturn.DoOnce && !_onInteractReturn.EventTriggered)
                        && _onInteractReturn.OnComplete.GetPersistentEventCount() > 0)
                    {
                        _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_interactionAnimator, _interactionStateHash,
                            _onInteractReturn.Delay, _onInteractReturn.OnComplete));
                    }
                }
            }
        }

        private void StopCallbackCoroutine()
        {
            if (_callbackCoroutine != null)
            {
                StopCoroutine(_callbackCoroutine);
                _callbackCoroutine = null;
            }
        }
    }
}