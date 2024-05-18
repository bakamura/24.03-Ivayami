using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableHighlight))]
    public class ActivableAnimation : Activable, IInteractable
    {
        [SerializeField] private bool _startActive;
        [SerializeField] private Animator _interactionAnimator;
        [SerializeField] private Animator _activateAnimator;
        [SerializeField] private AnimationEvent _onActivate;
        [SerializeField] private AnimationEvent _onInteract;

        private static int _interactBoolHash = Animator.StringToHash("interact");
        private static int _activateBoolHash = Animator.StringToHash("activate");

        private static int _interactionStateHash = Animator.StringToHash("interaction");
        private static int _activationStateHash = Animator.StringToHash("activation");

        private InteractableHighlight _interatctableHighlight;
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

        public InteractableHighlight InteratctableHighlight { get => _interatctableHighlight; }

        protected override void Awake()
        {
            base.Awake();
            if (_startActive) IsActive = true;
            _interatctableHighlight = GetComponent<InteractableHighlight>();
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
                if (_interactionAnimator) _interactionAnimator.SetBool(_interactBoolHash, !_interactionAnimator.GetBool(_interactBoolHash));
                CheckCallbacks(_interactBoolHash);
            }
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (_interactionAnimator)
            {
                if (!IsActive) _interactionAnimator.SetBool(_interactBoolHash, false);
                _interactionAnimator.SetBool(_activateBoolHash, IsActive);
            }
            if (_activateAnimator) _activateAnimator.SetBool(_activateBoolHash, IsActive);
            CheckCallbacks(_activateBoolHash);
        }

        private IEnumerator CallbackDelayCoroutine(Animator animator, int stateHash, float delay, UnityEvent unityEvent)
        {
            //waits for the animation transition to finish
            if (animator)
            {
                while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
                {
                    yield return null;
                }
            }
            yield return new WaitForSeconds(delay);
            unityEvent?.Invoke();
        }

        private void CheckCallbacks(int parameterHash)
        {
            if (parameterHash == _activateBoolHash && (!_onActivate.DoOnce || _onActivate.DoOnce && !_onActivate.EventTriggered)
                && _onActivate.OnComplete.GetPersistentEventCount() > 0)
            {
                StopCallbackCoroutine();
                _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_activateAnimator, _activationStateHash,
                    _activateAnimator ? _activateAnimator.GetCurrentAnimatorStateInfo(0).length + _onActivate.Delay : _onActivate.Delay,
                    _onActivate.OnComplete));
            }
            else if (parameterHash == _interactBoolHash && (!_onInteract.DoOnce || _onInteract.DoOnce && !_onInteract.EventTriggered)
                && _onInteract.OnComplete.GetPersistentEventCount() > 0)
            {
                StopCallbackCoroutine();
                _callbackCoroutine = StartCoroutine(CallbackDelayCoroutine(_interactionAnimator, _interactionStateHash,
                    _interactionAnimator ? _interactionAnimator.GetCurrentAnimatorStateInfo(0).length + _onInteract.Delay : _onInteract.Delay, 
                    _onInteract.OnComplete));
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