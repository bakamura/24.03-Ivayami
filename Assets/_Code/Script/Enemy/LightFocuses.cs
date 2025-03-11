using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine.Events;

namespace Ivayami.Enemy {
    public class LightFocuses : MonoSingleton<LightFocuses> {

        //public static UnityEvent OnChange { get; private set; }

        private Dictionary<string, Vector3> _focuses = new Dictionary<string, Vector3>();
        public Vector3[] Focuses { get { return _focuses.Values.ToArray(); } }

        public void FocusUpdate(string key, Vector3 position) {
            if (_focuses.ContainsKey(key)) _focuses[key] = position;
            else _focuses.Add(key, position);
        }

        public void FocusRemove(string key) {
            _focuses.Remove(key);
        }

        public Vector3 GetClosestPointTo(Vector3 position) {
            Vector3[] focuses = Focuses;
            if (focuses.Length <= 0) return Vector3.down;
            if (focuses.Length == 1) return focuses[0];
            int closest = 0;
            float closestDistance = Vector3.Distance(position, focuses[0]);
            float distanceCache;
            for (int i = 1; i < focuses.Length; i++) {
                distanceCache = Vector3.Distance(position, focuses[1]);
                if (closestDistance > distanceCache) {
                    closest = i;
                    closestDistance = distanceCache;
                }
            }
            return focuses[closest];
        }

    }
}
