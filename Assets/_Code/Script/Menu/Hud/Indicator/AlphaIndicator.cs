using UnityEngine;

namespace Ivayami.UI {
    public class AlphaIndicator : Indicator {

        [Header("Alpha")]

        private CanvasGroup _canvasGroup;

        protected override void Awake() {
            base.Awake();

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void FillUpdate(float value) {
            _canvasGroup.alpha = EvaluateCurve(value);
        }

        public void ForceDisplay(bool shouldForce) {
            _canvasGroup.enabled = !shouldForce;
        }

    }
}
