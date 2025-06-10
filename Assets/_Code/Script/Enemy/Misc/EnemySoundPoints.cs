using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Ivayami.Enemy
{
    public class EnemySoundPoints : MonoSingleton<EnemySoundPoints>
    {
        private Dictionary<string, SoundPointData> _soundPointsData = new Dictionary<string, SoundPointData>();
        public SoundPointData[] SoundPointsData => _soundPointsData.Values.ToArray();

        [Serializable]
        public struct SoundPointData
        {
            public Vector3 Position;
            [Min(0f)] public float Radius;
            public byte PlayCount;
            public static SoundPointData Empty = new SoundPointData();

            public bool IsValid()
            {
                return Position != Vector3.zero;
            }

            public bool Equals(SoundPointData data)
            {
                return Position == data.Position && PlayCount == data.PlayCount;
            }

            public SoundPointData(Vector3 pos, float radius, byte playCount)
            {
                Position = pos;
                Radius = radius;
                PlayCount = playCount;
            }

            public SoundPointData(Vector3 pos, float radius)
            {
                Position = pos;
                Radius = radius;
                PlayCount = 1;
            }
        }

        public static Action OnChange;

        public void UpdateSoundPoint(string key, SoundPointData data)
        {
            if (!_soundPointsData.ContainsKey(key)) _soundPointsData.Add(key, data);
            else
            {
                _soundPointsData[key] = new SoundPointData(data.Position, data.Radius, (byte)(_soundPointsData[key].PlayCount + 1 > byte.MaxValue ? 1 : (byte)(_soundPointsData[key].PlayCount + 1)));
            }
            OnChange?.Invoke();
        }

        public void RemoveSoundPoint(string key)
        {
            if (_soundPointsData.ContainsKey(key))
            {
                _soundPointsData.Remove(key);
                OnChange?.Invoke();
            }
        }

        public SoundPointData GetClosestPointToSoundPoint(Vector3 position, float radius)
        {
            SoundPointData[] sounds = SoundPointsData;
            if (sounds.Length <= 0) return SoundPointData.Empty;
            if (sounds.Length == 1 && Vector3.Distance(position, sounds[0].Position) <= sounds[0].Radius + radius) return sounds[0];
            else if (sounds.Length > 1)
            {
                int closest = 0;
                float closestDistance = 0;
                float distanceCache;
                for (int i = 1; i < sounds.Length; i++)
                {
                    distanceCache = Vector3.Distance(position, sounds[i].Position);
                    if (distanceCache <= sounds[i].Radius + radius && closestDistance > distanceCache)
                    {
                        closest = i;
                        closestDistance = distanceCache;
                    }
                }
                return sounds[closest];
            }
            else return SoundPointData.Empty;
        }
    }
}