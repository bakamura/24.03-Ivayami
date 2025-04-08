using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Enemy {
    public class Lightable : MonoBehaviour {

        protected HashSet<string> _illuminationSources { get; set; } = new HashSet<string>();
        public UnityEvent<LightFocuses.LightData> onIlluminated;

        protected virtual void Awake() {
            if (!TryGetComponent(out Collider collider)) Debug.LogWarning($"There is no Collider associated with Lightable '{name}'");
        }

        public void Iluminate(string key, LightFocuses.LightData lightData) {
            if (lightData.IsValid()) {
                if (_illuminationSources.Add(key)) {
                    if (_illuminationSources.Count == 1) onIlluminated.Invoke(lightData);
                }
                else Debug.LogWarning($"Tried to add illuminating '{key}' to {name} but is already illuminating!");
            }
            else {
                if (_illuminationSources.Remove(key)) {
                    if(_illuminationSources.Count == 0) onIlluminated.Invoke(lightData);
                }
                else Debug.LogWarning($"Tried to remove illuminating '{key}' to {name} but wasn't illuminating!");
            }
        }

    }
}
