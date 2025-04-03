using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine.Events;

namespace Ivayami.Enemy {
    public class LightFocuses : MonoSingleton<LightFocuses> {

        //public static UnityEvent OnChange { get; private set; }

        private Dictionary<string, LighData> _focuses = new Dictionary<string, LighData>();
        public LighData[] Focuses { get { return _focuses.Values.ToArray(); } }

        [System.Serializable]
        public struct LighData
        {
            public Vector3 Position;
            public float Radius;
            public static LighData Empty = new LighData();
            public LighData(Vector3 pos, float radius = 0)
            {
                Position = pos;
                Radius = radius;
            }
            public bool IsValid()
            {
                return Position != Vector3.zero;
            }
        }

        public void FocusUpdate(string key, LighData data) {
            if (_focuses.ContainsKey(key)) _focuses[key] = data;
            else _focuses.Add(key, data);
        }

        public void FocusRemove(string key) {
            _focuses.Remove(key);
        }

        public LighData GetClosestPointTo(Vector3 position) {
            LighData[] focuses = Focuses;
            if (focuses.Length <= 0) return LighData.Empty;
            if (focuses.Length == 1) return focuses[0];
            int closest = 0;
            float closestDistance = Vector3.Distance(position, focuses[0].Position);
            float distanceCache;
            for (int i = 1; i < focuses.Length; i++) {
                distanceCache = Vector3.Distance(position, focuses[1].Position);
                if (closestDistance > distanceCache) {
                    closest = i;
                    closestDistance = distanceCache;
                }
            }
            return focuses[closest];
        }

    }
}
