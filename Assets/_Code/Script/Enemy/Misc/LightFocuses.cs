using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ivayami.Enemy {
    public class LightFocuses : MonoSingleton<LightFocuses> {

        public static Action OnChange;

        private Dictionary<string, LightData> _lightAreaFocuses = new Dictionary<string, LightData>();
        private Dictionary<string, LightData> _lightPointFocuses = new Dictionary<string, LightData>();
        public LightData[] AreaFocuses { get { return _lightAreaFocuses.Values.ToArray(); } }
        public LightData[] PointFocuses { get { return _lightPointFocuses.Values.ToArray(); } }
 
        [System.Serializable]        
        public struct LightData
        {
            //public object Type;
            public Vector3 Position;
            public float Radius;
            public static LightData Empty = new LightData();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="type">EnemyLight, Lantern, null</param>
            /// <param name="pos"></param>
            /// <param name="radius"></param>
            public LightData(/*object type,*/ Vector3 pos, float radius = 0)
            {
                //Type = type;
                Position = pos;
                Radius = radius;
            }
            public bool IsValid()
            {
                return Position != Vector3.zero;
            }
        }

        public void LightAreaFocusUpdate(string key, LightData data) {
            if (_lightAreaFocuses.ContainsKey(key)) _lightAreaFocuses[key] = data;
            else _lightAreaFocuses.Add(key, data);
            OnChange?.Invoke();
        }

        public void LightAreaFocusRemove(string key) {
            _lightAreaFocuses.Remove(key);
            OnChange?.Invoke();
        }

        public void LightPointFocusUpdate(string key, LightData data)
        {
            if (_lightPointFocuses.ContainsKey(key)) _lightPointFocuses[key] = data;
            else _lightPointFocuses.Add(key, data);
            OnChange?.Invoke();
        }

        public void LightPointFocusRemove(string key)
        {
            _lightPointFocuses.Remove(key);
            OnChange?.Invoke();
        }

        public LightData GetClosestPointToAreaLight(Vector3 position, float radius) {
            LightData[] focuses = AreaFocuses;
            if (focuses.Length <= 0) return LightData.Empty;
            if (focuses.Length == 1 && Vector3.Distance(position, focuses[0].Position) <= focuses[0].Radius + radius) return focuses[0];
            else if (focuses.Length > 1)
            {
                int closest = 0;
                float closestDistance = Vector3.Distance(position, focuses[0].Position);
                float distanceCache;
                for (int i = 1; i < focuses.Length; i++)
                {
                    distanceCache = Vector3.Distance(position, focuses[i].Position);
                    if (distanceCache <= focuses[i].Radius + radius && closestDistance > distanceCache)
                    {
                        closest = i;
                        closestDistance = distanceCache;
                    }
                }
                return focuses[closest];
            }
            else return LightData.Empty;
        }

        public LightData GetClosestPointToPointLight(Vector3 position)
        {
            LightData[] focuses = PointFocuses;
            if (focuses.Length <= 0) return LightData.Empty;
            if (focuses.Length == 1) return focuses[0];
            int closest = 0;
            float closestDistance = Vector3.Distance(position, focuses[0].Position);
            float distanceCache;
            for (int i = 1; i < focuses.Length; i++)
            {
                distanceCache = Vector3.Distance(position, focuses[i].Position);
                if (closestDistance > distanceCache)
                {
                    closest = i;
                    closestDistance = distanceCache;
                }
            }
            return focuses[closest];
        }

        public LightData GetClosestPointToAllLights(Vector3 position, float radius)
        {
            LightData[] focuses = AreaFocuses;
            focuses = focuses.Union(PointFocuses).ToArray();
            if (focuses.Length <= 0) return LightData.Empty;
            if (focuses.Length == 1 && Vector3.Distance(position, focuses[0].Position) <= focuses[0].Radius + radius) return focuses[0];
            else if (focuses.Length > 1)
            {
                int closest = 0;
                float closestDistance = Vector3.Distance(position, focuses[0].Position);
                float distanceCache;
                for (int i = 1; i < focuses.Length; i++)
                {
                    distanceCache = Vector3.Distance(position, focuses[i].Position);
                    if (distanceCache <= focuses[i].Radius + radius && closestDistance > distanceCache)
                    {
                        closest = i;
                        closestDistance = distanceCache;
                    }
                }
                return focuses[closest];
            }
            else return LightData.Empty;
        }
    }
}
