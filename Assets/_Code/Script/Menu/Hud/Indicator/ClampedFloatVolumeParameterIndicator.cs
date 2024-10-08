using UnityEngine;
using UnityEngine.Rendering;

namespace Ivayami.UI {
    public class ClampedFloatVolumeParameterIndicator : VolumeParameterIndicator<ClampedFloatParameter, float> {

        public override void FillUpdate(float value) {
            _volumeParameter?.Override(Mathf.Lerp(_valueMin, _valueMax, EvaluateCurve(value)));
        }

    }
}