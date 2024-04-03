using Paranapiacaba.Player;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class StressIndicator : MonoBehaviour {

        [System.Serializable]
        private class Indicator {
            [SerializeField] private CanvasGroup _canvasGroup;
            [SerializeField] private AnimationCurve _animationCurve;
            [SerializeField, Range(0f, 1f)] private Vector2 _stressRangeAnimation;

            public CanvasGroup CanvasGroup { get { return _canvasGroup; } }
            public AnimationCurve AnimationCurve { get { return _animationCurve; } }
            public float StressMin { get { return _stressRangeAnimation.x; } }
            public float StressMax { get { return _stressRangeAnimation.y; } }
            }
        [SerializeField] private Indicator[] _stressIndicators;

        private void Awake() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateStressIndicators);
        }

        private void UpdateStressIndicators(float stress) {
            foreach (Indicator indicator in _stressIndicators) {
                indicator.CanvasGroup.alpha = indicator.AnimationCurve.Evaluate((stress - indicator.StressMin) / (indicator.StressMax - indicator.StressMin));
            }
        }

    }
}