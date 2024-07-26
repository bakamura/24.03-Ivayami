using UnityEngine;

namespace Ivayami.Enemy
{
    public interface IChangeTargetPoint
    {        
        public abstract void GoToPoint(Transform target, float speedIncrease, float durationInPlace);
    }
}