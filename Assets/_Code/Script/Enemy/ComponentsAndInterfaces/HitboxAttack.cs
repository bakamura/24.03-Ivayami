using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(BoxCollider))]
    public class HitboxAttack : MonoBehaviour
    {
        public UnityEvent OnTargetHit;
        public UnityEvent OnHitboxActivate;
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
            if (isActive) OnHitboxActivate?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerStress.Instance.AddStress(_currentStressIncrease);
            OnTargetHit?.Invoke();
        }
    }
}