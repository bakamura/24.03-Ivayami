using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Ivayami.Puzzle
{
    public class TriggerEvent : MonoBehaviour
    {
        [SerializeField] private TriggerTypes _triggerType;
        [SerializeField, Min(0f)] private float _delayToActivateEvent;
        [SerializeField] private UnityEvent _onExecute;
        [SerializeField] private string _optionalTag;
        private enum TriggerTypes
        {
            OnTriggerEnter,
            OnTriggerExit,
            OnCollisionEnter,
            OnCollisionExit
        }
        private Coroutine _eventDelayCoroutine;
        private bool _validTag;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggerType == TriggerTypes.OnTriggerEnter && _eventDelayCoroutine == null)
            {
                _validTag = string.IsNullOrEmpty(_optionalTag) || other.CompareTag(_optionalTag);
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_triggerType == TriggerTypes.OnTriggerExit && _eventDelayCoroutine == null)
            {
                _validTag = string.IsNullOrEmpty(_optionalTag) || other.CompareTag(_optionalTag);
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_triggerType == TriggerTypes.OnCollisionEnter && _eventDelayCoroutine == null)
            {
                _validTag = string.IsNullOrEmpty(_optionalTag) || collision.collider.CompareTag(_optionalTag);
                if (_validTag)
                {
                    if (_delayToActivateEvent > 0) _eventDelayCoroutine = StartCoroutine(EventDelayCoroutine());
                    else _onExecute?.Invoke();
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
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

        private IEnumerator EventDelayCoroutine()
        {
            yield return new WaitForSeconds(_delayToActivateEvent);
            _onExecute?.Invoke();
            _eventDelayCoroutine = null;
        }
    }
}
