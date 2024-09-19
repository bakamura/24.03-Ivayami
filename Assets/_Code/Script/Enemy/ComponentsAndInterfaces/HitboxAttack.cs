using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(BoxCollider))]
    public class HitboxAttack : MonoBehaviour
    {
        private BoxCollider _boxCollider
        {
            get
            {
                if (!m_boxCollider) m_boxCollider = GetComponent<BoxCollider>();
                return m_boxCollider;
            }
        }
        private BoxCollider m_boxCollider;
        private float _currentStressIncrease;

        public void UpdateHitbox(bool isActive, Vector3 center, Vector3 size, float stressIncrease)
        {
            _boxCollider.center = center;
            _boxCollider.size = size;
            _currentStressIncrease = stressIncrease;
            _boxCollider.enabled = isActive;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerStress.Instance.AddStress(_currentStressIncrease);
        }
    }
}