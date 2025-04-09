using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player.Ability;

namespace Ivayami.Puzzle {
    public class Lightable : MonoBehaviour {

        protected HashSet<string> _illuminationSources { get; set; } = new HashSet<string>();
        public UnityEvent<bool> onIlluminated;
        public UnityEvent<bool> onIlluminatedByLantern;
        public bool IsIlluminatedByLantern { get { return _illuminationSources.Contains(Lantern.ILLUMINATION_KEY); } }

        protected virtual void Awake() {
            if (!TryGetComponent(out Collider collider)) Debug.LogWarning($"There is no Collider associated with Lightable '{name}'");
        }

        public void Illuminate(string key, bool illuminated) {
            if (illuminated) {
                if (_illuminationSources.Add(key)) {
                    if (_illuminationSources.Count == 1) {
                        onIlluminated.Invoke(true);
                        if (key == Lantern.ILLUMINATION_KEY) onIlluminatedByLantern.Invoke(true);
                    }
                }
                else Debug.LogWarning($"Tried to add illuminating '{key}' to {name} but is already illuminating!");
            }
            else {
                if (_illuminationSources.Remove(key)) {
                    if (_illuminationSources.Count == 0) {
                        onIlluminated.Invoke(false);
                        if (key == Lantern.ILLUMINATION_KEY) onIlluminatedByLantern.Invoke(false);
                    }
                }
                else Debug.LogWarning($"Tried to remove illuminating '{key}' to {name} but wasn't illuminating!");
            }
        }

    }
}
