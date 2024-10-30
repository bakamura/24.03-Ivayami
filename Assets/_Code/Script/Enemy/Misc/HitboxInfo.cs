using UnityEngine;

namespace Ivayami.Enemy
{
    [System.Serializable]
    public struct HitboxInfo
    {
        [Range(0, 1)] public float MinInterval;
        [Range(0, 1)] public float MaxInterval;
        [SerializeField, Min(0f)] public float StressIncreaseOnEnter;
        [SerializeField, Min(0f), Tooltip("Damage each .5 seconds inside the damage area")] public float StressIncreaseOnStay;
        public Vector3 Center;
        public Vector3 Size;
    }
}
