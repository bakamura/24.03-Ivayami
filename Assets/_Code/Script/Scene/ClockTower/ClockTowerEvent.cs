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

        public virtual void StartEvent(float duration)
        {
            _eventDurationCoroutine = StartCoroutine(EventDurationCoroutine(duration));
        }

        public virtual void StopEvent()
        {            
            OnEndEvent?.Invoke();
            OnEndEvent = null;
        }

        public virtual void InterruptEvent()
        {
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