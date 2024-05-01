using UnityEngine;

namespace Ivayami.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour {

        [SerializeField] protected AnimationCurve _transitionCurve;
        [SerializeField] protected float _transitionDuration;
        public float TransitionDuration { get { return _transitionDuration; } }

        protected CanvasGroup _canvasGroup;

        protected virtual void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public abstract void Open();

        public abstract void Close();

    }
}