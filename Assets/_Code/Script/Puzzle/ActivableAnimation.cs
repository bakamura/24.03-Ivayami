using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class ActivableAnimation : Activable, IInteractable
    {
        [SerializeField] private bool _startActive;
        [SerializeField] private Animator _interactionAnimator;
        [SerializeField] private Animator _activateAnimator;
        [SerializeField] private AnimationEvent _onActivateAnimationEnd;
        [SerializeField] private AnimationEvent _onInteractAnimationEnd;

        private static int _interactBoolHash = Animator.StringToHash("interact");
        private static int _activateBoolHash = Animator.StringToHash("activate");

        private static int _interactionStateHash = Animator.StringToHash("interaction");
        private static int _activationStateHash = Animator.StringToHash("activation");

        private InteratctableHighlight _interatctableHighlight;
        private Coroutine _callbackCoroutine;

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

        public InteratctableHighlight InteratctableHighlight { get => _interatctableHighlight; }

        protected override void Awake()
        {
            base.Awake();
            if (_startActive) IsActive = true;
            _interatctableHighlight = GetComponent<InteratctableHighlight>();
            _onActivateAnimationEnd.Setup();
            _onInteractAnimationEnd.Setup();
        }

        private void OnDisable()
        {
            UpdateCallbackCoroutine();
        }

        public void Interact()
        {
            if (IsActive)
            {
                _interactionAnimator.SetBool(_interactBoolHash, !_interactionAnimator.GetBool(_interactBoolHash));
                CheckCallbacks(_interactBoolHash);
            }
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (!IsActive) _interactionAnimator.SetBool(_interactBoolHash, false);
            _activateAnimator.SetBool(_activateBoolHash, IsActive);
            _interactionAnimator.SetBool(_activateBoolHash, IsActive);
            CheckCallbacks(_activateBoolHash);     
        }

        private IEnumerator CallbackDelayCoroutine(Animator animator, int stateHash, float delay, UnityEvent unityEvent)
        {
            //waits for the animation transition to finish
            while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
            {
                yield return null;
            }
            yield return new WaitForSeconds(delay);
            unityEvent?.Invoke();
        }

        private void CheckCallbacks(int parameterHash)
        {
            if (parameterHash == _activateBoolHash && (!_onActivateAnimationEnd.DoOnce || _onActivateAnimationEnd.DoOnce && !_onActivateAnimationEnd.EventTriggered)
                && _onActivateAnimationEnd.OnComplete.GetPersistentEventCount() > 0)
            {
                UpdateCallbackCoroutine();
                _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_activateAnimator, _activationStateHash, 
                    _activateAnimator.GetCurrentAnimatorStateInfo(0).length + _onActivateAnimationEnd.Delay, _onActivateAnimationEnd.OnComplete));
            }
            else if (parameterHash == _interactBoolHash && (!_onInteractAnimationEnd.DoOnce || _onInteractAnimationEnd.DoOnce && !_onInteractAnimationEnd.EventTriggered)
                && _onInteractAnimationEnd.OnComplete.GetPersistentEventCount() > 0)
            {
                UpdateCallbackCoroutine();
                _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_interactionAnimator, _interactionStateHash, 
                    _interactionAnimator.GetCurrentAnimatorStateInfo(0).length + _onInteractAnimationEnd.Delay, _onInteractAnimationEnd.OnComplete));
            }
        }

        private void UpdateCallbackCoroutine()
        {
            if (_callbackCoroutine != null)
            {
                StopCoroutine(_callbackCoroutine);
                _callbackCoroutine = null;
            }
        }        
    }
}