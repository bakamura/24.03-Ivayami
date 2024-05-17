using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class TriggerEvent : MonoBehaviour
    {
        [SerializeField] private TriggerTypes _triggerType;
        [SerializeField] private UnityEvent _onExecute;
        [SerializeField] private string _optionalTag;        
        private enum TriggerTypes
        {
            OnTriggerEnter,
            OnTriggerExit,
            OnCollisionEnter,
            OnCollisionExit
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggerType == TriggerTypes.OnTriggerEnter)
            {
                if (string.IsNullOrEmpty(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
                else if (other.CompareTag(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_triggerType == TriggerTypes.OnTriggerExit)
            {
                if (string.IsNullOrEmpty(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
                else if (other.CompareTag(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_triggerType == TriggerTypes.OnCollisionEnter)
            {
                if (string.IsNullOrEmpty(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
                else if (collision.collider.CompareTag(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_triggerType == TriggerTypes.OnCollisionExit)
            {
                if (string.IsNullOrEmpty(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
                else if (collision.collider.CompareTag(_optionalTag))
                {
                    _onExecute?.Invoke();
                }
            }
        }
    }
}
