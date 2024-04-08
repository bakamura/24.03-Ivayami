using Paranapiacaba.Player;
using UnityEngine;

namespace Paranapiacaba.UI {
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

        [SerializeField] private Indicator[] _stressIndicators;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateStressIndicators);
        }

        private void UpdateStressIndicators(float stress) {
            foreach (Indicator indicator in _stressIndicators) {
                indicator.CanvasGroup.alpha = indicator.AnimationCurve.Evaluate((stress - indicator.StressMin) / (indicator.StressMax - indicator.StressMin));
            }
        }

    }
}