using UnityEngine;
using UnityEngine.Rendering;

namespace Ivayami.UI {
    public class Vector2VolumeParameterIndicator : VolumeParameterIndicator<Vector2Parameter, Vector2> {

        private float _circleLength;
        private float _angleCache;
        private Vector2 _vectorCache;

        protected override void Awake() {
            base.Awake();

            _circleLength = 2f * Mathf.PI;
        }

        public override void FillUpdate(float value) {
            _volumeParameter.value = ValueToVector2(EvaluateCurve(value));
        }

        private Vector2 ValueToVector2(float value) {
            _angleCache = value * _circleLength;
            _vectorCache[0] = Mathf.Cos(value);
            _vectorCache[1] = Mathf.Sin(value);
            return _vectorCache;
        }

    }
}