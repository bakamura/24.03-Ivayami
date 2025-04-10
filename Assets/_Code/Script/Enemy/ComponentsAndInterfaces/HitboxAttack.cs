using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;
using System.Collections;

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
        private const float _tickFrequency = .5f;
        private bool _isTargetInside;
        private Coroutine _damageCoroutine;
        private PlayerAnimation.DamageAnimation _currentDamageType;

        public void UpdateHitbox(bool isActive, Vector3 center, Vector3 size, float stressIncreaseOnEnter, float stressIncreaseOnStay, PlayerAnimation.DamageAnimation damageType)
        {
            _boxCollider.center = center;
            _boxCollider.size = size;
            _currentStressIncreaseOnEnter = stressIncreaseOnEnter;
            _currentStressIncreaseOnStay = stressIncreaseOnStay;
            _currentDamageType = damageType;
            _boxCollider.enabled = isActive;
            if (!isActive) _isTargetInside = false;
            if (!_previousState && isActive) OnHitboxActivate?.Invoke();
            if (_previousState && !isActive) OnHitboxDeactivate?.Invoke();
            _previousState = isActive;
        }

        private IEnumerator DamageCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_tickFrequency);
            while (_isTargetInside)
            {
                PlayerStress.Instance.AddStress(_currentStressIncreaseOnStay * _tickFrequency, damageType : _currentDamageType);
                yield return delay;
            }
            _damageCoroutine = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            _isTargetInside = true;
            if (_currentStressIncreaseOnEnter > 0) PlayerStress.Instance.AddStress(_currentStressIncreaseOnEnter, damageType: _currentDamageType);
            if (_currentStressIncreaseOnStay > 0 && _damageCoroutine == null) _damageCoroutine = StartCoroutine(DamageCoroutine());            
            OnTargetHit?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            _isTargetInside = false;
        }
    }
}