using UnityEngine;

namespace Ivayami.Enemy
{
    [System.Serializable]
    public struct HitboxInfo
    {
        public HitboxAttack Hitbox;
        [Range(0,1)] public int AnimationIndex;
        [Range(0, 1)] public float MinInterval;
        [Range(0, 1)] public float MaxInterval;
        [SerializeField, Min(0f)] public float StressIncreaseOnEnter;
        [SerializeField, Min(0f), Tooltip("Damage per second inside the damage area")] public float StressIncreaseOnStay;
        public Vector3 Center;
        [Tooltip("If true the size will be adjusted acording to the enemy attack animation, not all enemies suport this option")] public bool InterpolateSizeXAxisByAnimation;
        [Tooltip("If true the size will be adjusted acording to the enemy attack animation, not all enemies suport this option")] public bool InterpolateSizeYAxisByAnimation;
        [Tooltip("If true the size will be adjusted acording to the enemy attack animation, not all enemies suport this option")] public bool InterpolateSizeZAxisByAnimation;
        public Vector3 Size;

        public bool WillInterpolateAnySize()
        {
            return InterpolateSizeXAxisByAnimation || InterpolateSizeYAxisByAnimation || InterpolateSizeZAxisByAnimation;
        }        
    }
}
