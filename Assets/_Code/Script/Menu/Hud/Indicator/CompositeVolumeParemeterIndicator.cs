using UnityEngine;

namespace Ivayami.UI {
    public class CompositeVolumeParemeterIndicator : Indicator {

        private Indicator[] _indicators;

        protected override void Awake() {
            base.Awake();

            _indicators = GetComponents<Indicator>();
        }

        public override void FillUpdate(float value) {
            foreach(Indicator indicator in _indicators) indicator.FillUpdate(Mathf.Lerp(_min, _max, EvaluateCurve(value)));
        }

    }
}