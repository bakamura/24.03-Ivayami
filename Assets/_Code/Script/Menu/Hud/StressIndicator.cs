using UnityEngine;
using Ivayami.Player;

namespace Ivayami.UI {
    public class StressIndicator : MonoBehaviour {

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
            foreach (Indicator indicator in _stressIndicators) indicator.FillUpdate(stress);
        }

    }
}