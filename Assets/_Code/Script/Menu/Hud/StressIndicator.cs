using Ivayami.Player;
using UnityEngine;

namespace Ivayami.UI {
    public class StressIndicator : MonoBehaviour {

        [System.Serializable]
        private class Indicator {
            [SerializeField] private CanvasGroup _canvasGroup;
            [SerializeField] private AnimationCurve _animationCurve;
            [SerializeField] private float _stressAnimationMin;
            [SerializeField] private float _stressAnimationMax;

            public CanvasGroup CanvasGroup { get { return _canvasGroup; } }
            public AnimationCurve AnimationCurve { get { return _animationCurve; } }
            public float StressMin { get { return _stressAnimationMin; } }
            public float StressMax { get { return _stressAnimationMax; } }
        }

        [Header("Parameters")]

        [SerializeField] private Indicator[] _stressIndicators;
        [SerializeField, Range(0f, 1f)] private float _smoothFactor;

        [Header("Cache")]

        private float _displayTarget;
        private float _displayCurrent;
        private bool _gamePaused = false;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateDisplayTarget);
        }

        private void Update() {
            if (!_gamePaused) {
                _displayCurrent = Mathf.Lerp(_displayCurrent, _displayTarget, _smoothFactor);
                UpdateStressIndicators(_displayCurrent);
            }
        }

        private void UpdateDisplayTarget(float target) {
            _displayTarget = target;
        }

        private void UpdateStressIndicators(float stress) {
            foreach (Indicator indicator in _stressIndicators) {
                indicator.CanvasGroup.alpha = indicator.AnimationCurve.Evaluate((stress - indicator.StressMin) / (indicator.StressMax - indicator.StressMin));
                Logger.Log(LogType.UI, $"Stress Indicator '{indicator.CanvasGroup.name}' set to {indicator.CanvasGroup.alpha}");
            }
        }

    }
}