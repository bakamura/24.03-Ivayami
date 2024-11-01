using UnityEngine;

namespace Ivayami.UI {
    public abstract class Indicator : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] protected float _min;
        [SerializeField] protected float _max;

        [Header("Cache")]

        protected float _difference;

        protected virtual void Awake() {
            _difference = _max - _min;
        }

        public abstract void FillUpdate(float value);

        protected float EvaluateCurve(float value) {
            return _animationCurve.Evaluate((value - _min) / _difference);
        }

    }
}
