using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class TriggerEvent : MonoBehaviour
    {
        [SerializeField] private TriggerTypes _triggerType;
        [SerializeField] private bool _targetNeedToStayInside;
        [SerializeField, Min(0f)] private float _delayToActivateEvent;
        [SerializeField] private string _optionalTag;
        [SerializeField] private UnityEvent _onExecute;
        private enum TriggerTypes
        {
            OnTriggerEnter,
            OnTriggerExit,
            OnCollisionEnter,
            OnCollisionExit
        }
        private Coroutine _eventDelayCoroutine;
        private bool _validTag;
        private bool _isTargetInside;

        private void OnTriggerEnter(Collider other)
        {
            _validTag = string.IsNullOrEmpty(_optionalTag) || other.CompareTag(_optionalTag);
            if (_validTag) _isTargetInside = true;
            if (_triggerType == TriggerTypes.OnTriggerEnter && _eventDelayCoroutine == null)
            {
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _validTag = string.IsNullOrEmpty(_optionalTag) || other.CompareTag(_optionalTag);
            if (_validTag) _isTargetInside = false;
            if (_isTargetInside && _targetNeedToStayInside && _eventDelayCoroutine != null)
            {
                StopCoroutine(_eventDelayCoroutine);
                _eventDelayCoroutine = null;
            }
            if (_triggerType == TriggerTypes.OnTriggerExit && _eventDelayCoroutine == null)
            {
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            _validTag = string.IsNullOrEmpty(_optionalTag) || collision.collider.CompareTag(_optionalTag);
            if (_validTag) _isTargetInside = true;
            if (_triggerType == TriggerTypes.OnCollisionEnter && _eventDelayCoroutine == null)
            {
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            _validTag = string.IsNullOrEmpty(_optionalTag) || collision.collider.CompareTag(_optionalTag);
            if (_validTag) _isTargetInside = false;
            if (_triggerType == TriggerTypes.OnCollisionExit && _eventDelayCoroutine == null)
            {
                _validTag = string.IsNullOrEmpty(_optionalTag) || collision.collider.CompareTag(_optionalTag);
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnDisable()
        {
            if (_isTargetInside && (_triggerType == TriggerTypes.OnTriggerExit || _triggerType == TriggerTypes.OnCollisionExit))
            {
                _onExecute?.Invoke();
            }
        }

        private IEnumerator EventDelayCoroutine()
        {
            yield return new WaitForSeconds(_delayToActivateEvent);
            _onExecute?.Invoke();
            _eventDelayCoroutine = null;
        }
    }
}
