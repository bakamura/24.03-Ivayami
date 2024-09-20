using UnityEngine;

namespace Ivayami.Enemy
{
    [System.Serializable]
    public struct HitboxInfo
    {
        [Range(0, 1)] public float MinInterval;
        [Range(0, 1)] public float MaxInterval;
        [SerializeField, Min(0f)] public float StressIncrease;
        public Vector3 Center;
        public Vector3 Size;
    }
}
