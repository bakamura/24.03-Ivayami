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
        private float _currentStressIncreaseOnEnter;
        private float _currentStressIncreaseOnStay;
        private bool _previousState;

        public void UpdateHitbox(bool isActive, Vector3 center, Vector3 size, float stressIncreaseOnEnter, float stressIncreaseOnStay)
        {
            _boxCollider.center = center;
            _boxCollider.size = size;
            _currentStressIncreaseOnEnter = stressIncreaseOnEnter;
            _currentStressIncreaseOnStay = stressIncreaseOnStay;
            _boxCollider.enabled = isActive;
            if (!_previousState && isActive) OnHitboxActivate?.Invoke();
            if (_previousState && !isActive) OnHitboxDeactivate?.Invoke();
            _previousState = isActive;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentStressIncreaseOnEnter > 0) PlayerStress.Instance.AddStress(_currentStressIncreaseOnEnter);
            OnTargetHit?.Invoke();
        }

        private void OnTriggerStay(Collider other)
        {
            if (_currentStressIncreaseOnStay > 0) PlayerStress.Instance.AddStress(_currentStressIncreaseOnStay);
        }
    }
}