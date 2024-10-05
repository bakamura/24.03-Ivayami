using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace Ivayami.UI {
    public abstract class VolumeParameterIndicator<T1, T2> : Indicator where T1 : VolumeParameter {

        [Header("Volume Parameter")]

        [SerializeField] private string _volumeComponentName;
        [SerializeField] private int _volumeParameterId;

        [SerializeField] protected T2 _valueMin;
        [SerializeField] protected T2 _valueMax;

        [Header("Cache")]

        protected T1 _volumeParameter;

        protected override void Awake() {
            base.Awake();

            //foreach(VolumeParameter parameter in GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType().Name == _volumeComponentName).parameters) Debug.Log(name);
            _volumeParameter = (GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType().Name == _volumeComponentName).parameters[_volumeParameterId]) as T1;
        }

    }
}
