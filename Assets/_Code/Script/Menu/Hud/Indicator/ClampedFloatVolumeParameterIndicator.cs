using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Ivayami.UI {
    public class ClampedFloatVolumeParameterIndicator : VolumeParameterIndicator<ClampedFloatParameter, float> {

        public override void FillUpdate(float value) {
            Debug.Log(Mathf.Lerp(_valueMin, _valueMax, EvaluateCurve(value)));
            GetComponent<Volume>().profile.TryGet(out ColorAdjustments colorAdjustments); // Testing
            colorAdjustments.saturation.Override(Mathf.Lerp(_valueMin, _valueMax, EvaluateCurve(value))); // Testing 
            //_volumeParameter.Override(Mathf.Lerp(_valueMin, _valueMax, EvaluateCurve(value)));
            Debug.Log(_volumeParameter.value);
        }

    }
}