using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ivayami.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour {

        [Header("Menu")]

        [SerializeField] private bool _interactable;

        [field: SerializeField] public UnityEvent OnTransitionStart { get; private set; }
        [field: SerializeField] public UnityEvent OnTransitionEnd { get; private set; }

        [field: SerializeField] public UnityEvent OnOpenStart { get; private set; }
        [field: SerializeField] public UnityEvent OnOpenEnd { get; private set; }

        [field: SerializeField] public UnityEvent OnCloseStart { get; private set; }
        [field: SerializeField] public UnityEvent OnCloseEnd { get; private set; }

        [SerializeField] protected AnimationCurve _transitionCurve;
        [SerializeField, Min(0.01f)] protected float _transitionDuration;
        [field: SerializeField] public Selectable InitialSelected { get; protected set; }

        protected CanvasGroup _canvasGroup;
        protected Coroutine _routine;
        protected bool _isOpening;

        protected virtual void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();

            OnTransitionStart.AddListener(() => {
                if (_isOpening) OnOpenStart.Invoke();
                else OnCloseStart.Invoke();
            });
            OnTransitionEnd.AddListener(() => {
                if (_isOpening) OnOpenEnd.Invoke();
                else OnCloseEnd.Invoke();
            });
            OnTransitionEnd.AddListener(() => _routine = null);
            OnOpenStart.AddListener(() => _isOpening = true);
            OnCloseStart.AddListener(() => _isOpening = false);
            OnCloseStart.AddListener(() => InteractableUpdate(false));
            OnOpenEnd.AddListener(() => InteractableUpdate(_interactable));
        }

        public virtual void Open() {
            CancelPrevious();
            _isOpening = true;
            _routine = StartCoroutine(Transition());

            Logger.Log(LogType.UI, $"Open Menu '{name}'");
        }

        public virtual void Close() {
            CancelPrevious();
            _isOpening = false;
            _routine = StartCoroutine(Transition());

            Logger.Log(LogType.UI, $"Close Menu '{name}'");
        }

        private IEnumerator Transition() {
            OnTransitionStart.Invoke();

            float currentPhase = 0f;
            while (currentPhase < 1f) {
                currentPhase += Time.deltaTime / _transitionDuration;
                TransitionBehaviour(currentPhase);

                yield return null;
            }

            OnTransitionEnd.Invoke();
        }

        private void CancelPrevious() {
            if (_routine != null) {
                if (_isOpening) OnOpenEnd.Invoke();
                else OnCloseEnd.Invoke();
                StopCoroutine(_routine);
            }
        }

        private void InteractableUpdate(bool isInteractable) {
            _canvasGroup.interactable = isInteractable;
            _canvasGroup.blocksRaycasts = isInteractable;
        }

        protected virtual void TransitionBehaviour(float currentPhase) { }

    }
}