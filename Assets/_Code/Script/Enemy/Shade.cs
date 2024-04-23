using System.Collections;
using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Enemy
{
    public class Shade : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _durationInScreenOnSight;
        [SerializeField, Min(0)] private float _durationInScreenOnTargetClose;
        [SerializeField, Min(0)] private float _cooldowToShowAgain;
        [SerializeField] private float _stressIncrease;
        [SerializeField] private MeshRenderer _visual;

        private bool _playerInsideArea;
        private WaitForSeconds _delayOnSight;
        private WaitForSeconds _delayOnTargetClose;
        private WaitForSeconds _delayToShowAgain;
        private Coroutine _hideCoroutine;
        private bool _isVisible = true;
        private bool _inCooldown = false;
        private bool _isBeeingSeen;

        private void Awake()
        {
            _delayOnSight = new WaitForSeconds(_durationInScreenOnSight);
            _delayOnTargetClose = new WaitForSeconds(_durationInScreenOnTargetClose);
            _delayToShowAgain = new WaitForSeconds(_cooldowToShowAgain);
        }

        private void OnTriggerEnter(Collider other)
        {
            _playerInsideArea = true;
            TryStressIncrease();
        }

        private void OnTriggerExit(Collider other)
        {
            _playerInsideArea = false;
        }

        private void TryStressIncrease()
        {
            if (_playerInsideArea && !_inCooldown)
            {
                PlayerStress.Instance.AddStress(_stressIncrease);
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                _hideCoroutine = StartCoroutine(HideCoroutine(_delayOnTargetClose));
            }
        }

        private void OnBecameVisible()
        {
            _isBeeingSeen = true;
            if (!_playerInsideArea && !_inCooldown && _isVisible)
            {
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                _hideCoroutine = StartCoroutine(HideCoroutine(_delayOnSight));
            }
        }

        private void OnBecameInvisible()
        {
            _isBeeingSeen = false;
            if (!_playerInsideArea && !_inCooldown)
            {
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                UpdateVidibility(true);
            }
        }

        private void UpdateVidibility(bool isVisible)
        {
            _isVisible = isVisible;
            _visual.material.color = new Color(_visual.material.color.r, _visual.material.color.g, _visual.material.color.b, isVisible ? 1 : 0);
        }

        private IEnumerator HideCoroutine(WaitForSeconds delay)
        {
            yield return delay;
            UpdateVidibility(false);
            _inCooldown = true;
            yield return _delayToShowAgain;
            while (_isBeeingSeen)
            {
                yield return null;
            }
            _hideCoroutine = null;
            _inCooldown = false;
            UpdateVidibility(true);
            TryStressIncrease();
        }
    }
}