using UnityEngine;

namespace Ivayami.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/MovementData", fileName = "NewMovementData")]
    public class EnemyMovementData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _acceleration;
        [SerializeField, Min(0f)] private float _walkSpeed;
        [SerializeField, Min(0f)] private float _chaseSpeed;
        [SerializeField, Min(0f)] private float _rotationSpeed;

        public float Acceleration => _acceleration;
        public float WalkSpeed => _walkSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float RotationSpeed => _rotationSpeed;
    }
}