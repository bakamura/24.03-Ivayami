using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;

namespace Ivayami.UI {
    public class StressIndicatorSmoother : MonoSingleton<StressIndicatorSmoother> {

        public UnityEvent<float> OnStressSmoothed { get; private set; } = new UnityEvent<float>();
        [SerializeField, Range(0f, 1f)] private float _smoothFactor;
        [SerializeField, Min(0f)] private float _minDisplacement;
        private float _target;
        private float _current;
        private Coroutine _coroutine;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(SetTarget);
        }

        private void SetTarget(float newTarget) {
            _target = newTarget;
            if (_coroutine == null) _coroutine = StartCoroutine(SmoothToTarget());
        }

        private IEnumerator SmoothToTarget() {
            float frameDisplacement;
            float minDisplacementInFrame;
            while (Mathf.Abs(_target - _current) > _minDisplacement) {
                frameDisplacement = (_target - _current) * _smoothFactor * Time.deltaTime;
                minDisplacementInFrame = _minDisplacement * Time.deltaTime;
                if (Mathf.Abs(frameDisplacement) < minDisplacementInFrame) frameDisplacement = Mathf.Sign(frameDisplacement) * minDisplacementInFrame;
                _current += frameDisplacement;
                OnStressSmoothed.Invoke(_current);

                yield return null;
            }
            _current = _target;
            OnStressSmoothed.Invoke(_current);
            _coroutine = null;
        }

    }
}
