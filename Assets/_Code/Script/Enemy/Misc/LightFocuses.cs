using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Enemy {
    public class LightFocuses : MonoSingleton<LightFocuses> {

        public static UnityEvent OnChange { get; private set; } = new UnityEvent();

        private Dictionary<string, LightData> _focuses = new Dictionary<string, LightData>();
        public LightData[] Focuses { get { return _focuses.Values.ToArray(); } }

        [System.Serializable]        
        public struct LightData
        {
            public object Type;
            public Vector3 Position;
            public float Radius;
            public static LightData Empty = new LightData();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="type">EnemyLight, Lantern, null</param>
            /// <param name="pos"></param>
            /// <param name="radius"></param>
            public LightData(object type, Vector3 pos, float radius = 0)
            {
                Type = type;
                Position = pos;
                Radius = radius;
            }
            public bool IsValid()
            {
                return Position != Vector3.zero;
            }
        }

        public void FocusUpdate(string key, LightData data) {
            if (_focuses.ContainsKey(key)) _focuses[key] = data;
            else _focuses.Add(key, data);
            OnChange.Invoke();
        }

        public void FocusRemove(string key) {
            _focuses.Remove(key);
            OnChange.Invoke();
        }

        public LightData GetClosestPointTo(Vector3 position) {
            LightData[] focuses = Focuses;
            if (focuses.Length <= 0) return LightData.Empty;
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

        public bool IsPointInsideLightRange(Vector3 position, float range) {
            foreach(LightData lightData in _focuses.Values) if(Vector2.Distance(position, lightData.Position) < range + lightData.Radius) return true;
            return false;
        }

    }
}
