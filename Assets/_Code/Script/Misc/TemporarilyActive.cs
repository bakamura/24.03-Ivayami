using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Misc
{
    public sealed class TemporarilyActive : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField] private bool _activateOnEndTimerEventOnInteruption;
        [SerializeField] private UnityEvent _onStartTimer;
        [SerializeField] private UnityEvent _onEndTimer;

        private Coroutine _timerCoroutine;

        private void OnDisable()
        {
            StopTimer();
        }

        public void StartTimer()
        {
            if(_timerCoroutine == null && gameObject.activeInHierarchy)
            {
                _onStartTimer?.Invoke();
               _timerCoroutine = StartCoroutine(TimerCoroutine());
            }
        }

        public void StopTimer()
        {
            if(_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
                if (_activateOnEndTimerEventOnInteruption) _onEndTimer?.Invoke();
            }
        }

        private IEnumerator TimerCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_duration);
            yield return delay;
            _timerCoroutine = null;
            _onEndTimer?.Invoke();
        }
    }
}