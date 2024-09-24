using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;

namespace Ivayami.UI
{
    public class ScreenFade : MonoBehaviour {
        private static bool _isFading;
        [SerializeField, Min(0f)] private float _delayBeforeStartFade;
        [SerializeField] private UnityEvent _onAfterDelayBeforeFadeStart;
        [SerializeField, Min(0f)] private float _duration = 1f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private UnityEvent _onFadeEnd;
        [SerializeField] private bool _debugLogs;

        private Action _onFadeEndCallback;
        private Coroutine _delayFadeStartCoroutine;
        private bool _isFadeIn;
        private bool _callbackExecuted;
        private const string BLOCK_KEY = "ScreenFade";

        [ContextMenu("FadeIn")]
        public void FadeIn() {
            if (!_isFading) {
                if (_debugLogs) Debug.Log($"Fade In Request From {gameObject.name}");
                _isFading = true;
                _isFadeIn = true;
                _delayFadeStartCoroutine = StartCoroutine(FadeStartDelayCoroutine());
                PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false);
            }
        }

        [ContextMenu("FadeOut")]
        public void FadeOut() {
            if (!_isFading) {
                if (_debugLogs) Debug.Log($"Fade Out Request From {gameObject.name}");
                _isFading = true;
                _isFadeIn = false;
                _delayFadeStartCoroutine = StartCoroutine(FadeStartDelayCoroutine());
                PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, true);
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

        private IEnumerator FadeStartDelayCoroutine() {
            yield return new WaitForSeconds(_delayBeforeStartFade);
            _delayFadeStartCoroutine = null;
            if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
            SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
            DelayFadeStartEnd();
            if (_isFadeIn) {
                SceneTransition.Instance.OnOpenEnd.AddListener(FadeEnd);
                SceneTransition.Instance.Open();
            }
            else {
                SceneTransition.Instance.OnCloseEnd.AddListener(FadeEnd);
                SceneTransition.Instance.Close();
            }
        }

        private void DelayFadeStartEnd()
        {
            _onAfterDelayBeforeFadeStart?.Invoke();
        }
        
        private void FadeEnd()
        {
            if (_debugLogs) Debug.Log($"Fade End From {gameObject.name}");
            _callbackExecuted = true;
            _onFadeEndCallback?.Invoke();
            _onFadeEnd?.Invoke();
            _onFadeEndCallback = null;
            _isFading = false;
            if (_isFadeIn) {
                SceneTransition.Instance.OnOpenEnd.RemoveListener(FadeEnd);
            }
            else {
                SceneTransition.Instance.OnCloseEnd.RemoveListener(FadeEnd);
            }
        }

        private void OnDisable()
        {
            if (!_callbackExecuted)
            {
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