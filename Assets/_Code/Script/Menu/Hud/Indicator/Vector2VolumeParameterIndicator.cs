using UnityEngine;
using UnityEngine.Rendering;

namespace Ivayami.UI {
    public class Vector2VolumeParameterIndicator : VolumeParameterIndicator<Vector2Parameter, Vector2> {

        [Tooltip("Leave at 0 for linear transition")]
        [SerializeField] private float _spinRate;

        private float _circleLength;
        private float _angleCache;
        private Vector2 _vectorCache;
        private Vector2 _vectorHalf = Vector2.one / 2f;

        protected override void Awake() {
            base.Awake();

            _circleLength = 2f * Mathf.PI;
        }

        public override void FillUpdate(float value) {
            _volumeParameter.value = _spinRate > 0 ? AngleToVector2Spin(EvaluateCurve(value)) : Vector2.Lerp(_valueMin, _valueMax, EvaluateCurve(value));
        }

        private Vector2 AngleToVector2Spin(float value) {
            if (value <= 0) return _vectorHalf;
            _angleCache += value * _spinRate * Time.deltaTime;
            if (_angleCache > 360f) _angleCache -= 360f;
            _vectorCache = Vector2.Lerp(_valueMin, _valueMax, value);
            _vectorCache[0] *= Mathf.Cos(_angleCache) / 2f;
            _vectorCache[1] *= Mathf.Sin(_angleCache) / 2f;
            _vectorCache += _vectorHalf;
            return _vectorCache;
        }

    }
}