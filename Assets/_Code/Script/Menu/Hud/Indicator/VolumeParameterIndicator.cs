using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ivayami.UI {
    [RequireComponent(typeof(Volume))]
    public abstract class VolumeParameterIndicator<T1, T2> : Indicator where T1 : VolumeParameter {

        [Header("Volume Parameter")]

        [SerializeField] private string _volumeComponentName;
        [SerializeField] private string _volumeParameterName;

        [SerializeField] protected T2 _valueMin;
        [SerializeField] protected T2 _valueMax;

        [Header("Cache")]

        protected T1 _volumeParameter;

        protected override void Awake() {
            base.Awake();

            VolumeComponent component = GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType().Name == _volumeComponentName);
            if (component != null) {
                _volumeParameter = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(field => field.Name.Equals(_volumeParameterName)).GetValue(component) as T1;
                if (_volumeParameter == null) Debug.LogError($"No Volume Parameter named '{_volumeParameterName}' found");
            }
            else Debug.LogError($"No Volume Component named '{_volumeComponentName}' found in Profile {GetComponent<Volume>().profile.name} (in {name})");
        }

    }
}
