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
        public UnityEvent OnHitboxDeactivate;
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
        private bool _previousState;

        public void UpdateHitbox(bool isActive, Vector3 center, Vector3 size, float stressIncrease)
        {
            _boxCollider.center = center;
            _boxCollider.size = size;
            _currentStressIncrease = stressIncrease;
            _boxCollider.enabled = isActive;
            if (!_previousState && isActive) OnHitboxActivate?.Invoke();
            if (_previousState && !isActive) OnHitboxDeactivate?.Invoke();
            _previousState = isActive;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerStress.Instance.AddStress(_currentStressIncrease);
            OnTargetHit?.Invoke();
        }
    }
}