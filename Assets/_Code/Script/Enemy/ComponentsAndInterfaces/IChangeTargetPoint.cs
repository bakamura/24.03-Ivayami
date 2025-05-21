using UnityEngine;

namespace Ivayami.Enemy
{
    public interface IChangeTargetPoint
    {
        public Vector3 CurrentPosition { get; }
        public abstract void GoToPoint(Transform target, float speedIncrease, float durationInPlace);
    }
}