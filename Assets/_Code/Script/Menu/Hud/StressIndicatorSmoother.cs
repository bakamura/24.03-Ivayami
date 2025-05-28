using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;

namespace Ivayami.UI {
    public class StressIndicatorSmoother : MonoSingleton<StressIndicatorSmoother> {

        public UnityEvent<float> OnStressSmoothed { get; private set; } = new UnityEvent<float>();
        [SerializeField, Range(0f, 1f)] private float _smoothFactor;
        [SerializeField, Min(0f)] private float _snapValue;
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
            while (Mathf.Abs(_target - _current) > _snapValue) {
                _current = Mathf.Lerp(_current, _target, _smoothFactor * Time.deltaTime);
                OnStressSmoothed.Invoke(_current);

                yield return null;
            }
            OnStressSmoothed.Invoke(_target);
            _coroutine = null;
        }

    }
}
