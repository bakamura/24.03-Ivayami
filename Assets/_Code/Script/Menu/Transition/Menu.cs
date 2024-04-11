using UnityEngine;

namespace Paranapiacaba.UI {
    public abstract class Menu : MonoBehaviour {

        [SerializeField] protected AnimationCurve _transitionCurve;
        [SerializeField] protected float _transitionDuration;
        public float TransitionDuration { get { return _transitionDuration; } }

        public abstract void Open();

        public abstract void Close();

    }
}