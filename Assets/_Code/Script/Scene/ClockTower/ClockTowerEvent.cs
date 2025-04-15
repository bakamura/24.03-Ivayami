using UnityEngine;
using System;
using System.Collections;

namespace Ivayami.Scene
{
    public abstract class ClockTowerEvent : MonoBehaviour
    {
        public Action OnEndEvent;
        public Action OnInterruptEvent;
        private Coroutine _eventDurationCoroutine;
        private bool _debugLogs;

        public virtual void StartEvent(float duration, bool debugLogs)
        {
            _debugLogs = debugLogs;
            if(_debugLogs) Debug.Log($"Event {name} started, will end in {duration} seconds");
            _eventDurationCoroutine = StartCoroutine(EventDurationCoroutine(duration));
        }

        protected virtual void StopEvent()
        {
            if (_debugLogs) Debug.Log($"Event {name} ended");
            OnEndEvent?.Invoke();
            OnEndEvent = null;
        }

        public virtual void InterruptEvent()
        {
            if (_debugLogs) Debug.Log($"Event {name} interrupted");
            if (_eventDurationCoroutine != null)
            {
                StopCoroutine(_eventDurationCoroutine);
                _eventDurationCoroutine = null;
            }
            OnInterruptEvent?.Invoke();
            OnInterruptEvent = null;
        }

        protected virtual IEnumerator EventDurationCoroutine(float duration)
        {
            WaitForSeconds delayEnd = new WaitForSeconds(duration);
            yield return delayEnd;
            _eventDurationCoroutine = null;
            StopEvent();
        }        
    }
}