using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paranapiacaba.Player;

namespace Paranapiacaba.Enemy
{
    public class Shade : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _durationInScreenOnSight;
        [SerializeField, Min(0)] private float _durationInScreenOnTargetClose;
        [SerializeField] private float _stressIncrease;
        [SerializeField] private MeshRenderer _visual;

        private bool _playerInsideArea;
        private WaitForSeconds _delayOnSight;
        private WaitForSeconds _delayOnTargetClose;
        private Coroutine _hideCoroutine;
        private bool _isVisible = true;

        private void Awake()
        {
            _delayOnSight = new WaitForSeconds(_durationInScreenOnSight);
            _delayOnTargetClose = new WaitForSeconds(_durationInScreenOnTargetClose);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !_playerInsideArea)
            {
                _playerInsideArea = true;
                PlayerStress.Instance.AddStress(_stressIncrease);
                if (_hideCoroutine != null)
                {

                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                Debug.Log("HideByClose");
                _hideCoroutine = StartCoroutine(HideCoroutine(_delayOnTargetClose));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInsideArea = false;
                if (!_isVisible)
                {
                    _isVisible = true;
                    _visual.material.color = new Color(_visual.material.color.r, _visual.material.color.g, _visual.material.color.b, 1);
                }
            }
        }

        private void OnBecameVisible()
        {
            if (!_playerInsideArea)
            {
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                if (_isVisible) _hideCoroutine = StartCoroutine(HideCoroutine(_delayOnSight));
            }
        }

        private void OnBecameInvisible()
        {
            if (!_playerInsideArea)
            {
                _isVisible = true;
                _visual.material.color = new Color(_visual.material.color.r, _visual.material.color.g, _visual.material.color.b, 1);
            }
        }

        private IEnumerator HideCoroutine(WaitForSeconds delay)
        {
            yield return delay;
            _isVisible = false;
            _visual.material.color = new Color(_visual.material.color.r, _visual.material.color.g, _visual.material.color.b, 0);
            _hideCoroutine = null;
        }
    }
}