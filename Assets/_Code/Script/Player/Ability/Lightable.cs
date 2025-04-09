using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Enemy {
    public class Lightable : MonoBehaviour {

        protected HashSet<string> _illuminationSources { get; set; } = new HashSet<string>();
        public UnityEvent<bool> onIlluminated;
        //public UnityEvent<LightFocuses.LightData> onIlluminated;

        //protected virtual void Awake() {
        //    if (!TryGetComponent(out Collider collider)) Debug.LogWarning($"There is no Collider associated with Lightable '{name}'");
        //}

        public void Iluminate(string key, bool illuminated/*, LightFocuses.LightData lightData*/) {
            if (illuminated) {
                if (_illuminationSources.Add(key)) {
                    if (_illuminationSources.Count == 1) onIlluminated.Invoke(true);
                }
                else Debug.LogWarning($"Tried to add illuminating '{key}' to {name} but is already illuminating!");
            }
            else {
                if (_illuminationSources.Remove(key)) {
                    if(_illuminationSources.Count == 0) onIlluminated.Invoke(false);
                }
                else Debug.LogWarning($"Tried to remove illuminating '{key}' to {name} but wasn't illuminating!");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!TryGetComponent(out Collider collider)) Debug.LogWarning($"There is no Collider associated with Lightable '{name}'");
        }
#endif
    }
}
