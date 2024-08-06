using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Ivayami.UI
{
    public class ScreenFade : MonoBehaviour
    {
        private static bool _isFading;
        [SerializeField, Min(0f)] private float _delayBeforeStartFade;
        [SerializeField] private UnityEvent _onDelayBeforeFadeEnd;
        [SerializeField, Min(0f)] private float _duration = 1f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private UnityEvent _onFadeEnd;
        [SerializeField] private bool _debugLogs;

        private AnimationCurve _previousCurve;
        private Action _onFadeEndCallback;
        private Coroutine _waitEndCoroutine;
        private Coroutine _delayFadeStartCoroutine;
        private bool _isFadeIn;
        [ContextMenu("FadeIn")]
        public void FadeIn()
        {
            if (!_isFading)
            {
                if (_debugLogs) Debug.Log($"Fade In Request From {gameObject.name}");
                _isFading = true;
                _isFadeIn = true;
                _delayFadeStartCoroutine = StartCoroutine(FadeStartDelayCoroutine());
            }
        }
        [ContextMenu("FadeOut")]
        public void FadeOut()
        {
            if (!_isFading)
            {
                if (_debugLogs) Debug.Log($"Fade Out Request From {gameObject.name}");
                _isFading = true;
                _isFadeIn = false;
                _delayFadeStartCoroutine = StartCoroutine(FadeStartDelayCoroutine());
            }
        }

        //public void FadeIn(Action onFadeEnd = null)
        //{
        //    if (!_isFading)
        //    {
        //        _isFading = true;
        //        if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
        //        _previousCurve = SceneTransition.Instance.TransitionCurve;
        //        SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
        //        SceneTransition.Instance.Menu.Open();
        //        StartCoroutine(WaitFadeCoroutine(onFadeEnd));
        //    }
        //}
        //public void FadeOut(Action onFadeEnd = null)
        //{
        //    if (!_isFading)
        //    {
        //        _isFading = true;
        //        if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
        //        _previousCurve = SceneTransition.Instance.TransitionCurve;
        //        SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
        //        SceneTransition.Instance.Menu.Close();
        //        _waitEndCoroutione = StartCoroutine(WaitFadeCoroutine(onFadeEnd));
        //    }
        //}

        private IEnumerator FadeStartDelayCoroutine()
        {
            yield return new WaitForSeconds(_delayBeforeStartFade);
            _delayFadeStartCoroutine = null;
            if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
            _previousCurve = SceneTransition.Instance.TransitionCurve;
            SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
            if (_isFadeIn) SceneTransition.Instance.Menu.Open();
            else SceneTransition.Instance.Menu.Close();
            DelayFadeStartEnd();
            _waitEndCoroutine = StartCoroutine(WaitFadeCoroutine());
        }

        private IEnumerator WaitFadeCoroutine(Action onFadeEnd = null)
        {
            float count = 0;
            _onFadeEndCallback = onFadeEnd;
            while (count < SceneTransition.Instance.Menu.TransitionDuration)
            {
                count += Time.deltaTime;
                yield return null;
            }
            _waitEndCoroutine = null;
            FadeEnd();
        }

        private void DelayFadeStartEnd()
        {
            _onDelayBeforeFadeEnd?.Invoke();
        }

        private void FadeEnd()
        {
            if (_debugLogs) Debug.Log($"Fade End From {gameObject.name}");
            SceneTransition.Instance.SetAnimationCurve(_previousCurve);
            _onFadeEndCallback?.Invoke();
            _onFadeEnd?.Invoke();
            _onFadeEndCallback = null;
            _isFading = false;
        }

        private void OnDisable()
        {
            if (_waitEndCoroutine != null)
            {
                _waitEndCoroutine = null;
                FadeEnd();
            }
            if (_delayFadeStartCoroutine != null)
            {
                _delayFadeStartCoroutine = null;
                DelayFadeStartEnd();
                _isFading = false;
            }
        }
    }
}