using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Ivayami.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour {

        [SerializeField] protected AnimationCurve _transitionCurve;
        [field: SerializeField, FormerlySerializedAs("_transitionDuration")] public float TransitionDuration { get; protected set; }
        [field: SerializeField] public Selectable InitialSelected { get; protected set; }

        protected CanvasGroup _canvasGroup;

        protected virtual void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public abstract void Open();

        public abstract void Close();

    }
}