using UnityEngine;

namespace Ivayami.Enemy
{
    public interface IIluminatedEnemy
    {
        public void ChangeSpeed(float speed);
        public void UpdateBehaviour(bool canWalkPath, bool canChaseTarget);
        public void ChangeTargetPoint(Vector3 targetPoint);
        public float CurrentSpeed { get; }
    }
}