using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Ivayami.UI
{
    public class ScreenFade : MonoBehaviour
    {
        private static bool _isFading;
        [SerializeField, Min(0f)] private float _duration = 1f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0,0,1,1);
        [SerializeField] private UnityEvent _onFadeEnd;

        private AnimationCurve _previousCurve;
        private Action _onFadeEndCallback;
        private Coroutine _waitEndCoroutione;
        [ContextMenu("FadeIn")]
        public void FadeIn()
        {
            if (!_isFading)
            {
                _isFading = true;
                if(_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
                _previousCurve = SceneTransition.Instance.TransitionCurve;
                SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
                SceneTransition.Instance.Menu.Open();
                _waitEndCoroutione = StartCoroutine(WaitFadeCoroutine());
            }
        }
        [ContextMenu("FadeOut")]
        public void FadeOut()
        {
            if (!_isFading)
            {
                _isFading = true;
                if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
                _previousCurve = SceneTransition.Instance.TransitionCurve;
                SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
                SceneTransition.Instance.Menu.Close();
                StartCoroutine(WaitFadeCoroutine());
            }
        }

        public void FadeIn(Action onFadeEnd = null)
        {
            if (!_isFading)
            {
                _isFading = true;
                if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
                _previousCurve = SceneTransition.Instance.TransitionCurve;
                SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
                SceneTransition.Instance.Menu.Open();
                StartCoroutine(WaitFadeCoroutine(onFadeEnd));
            }
        }
        public void FadeOut(Action onFadeEnd = null)
        {
            if (!_isFading)
            {
                _isFading = true;
                if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
                _previousCurve = SceneTransition.Instance.TransitionCurve;
                SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
                SceneTransition.Instance.Menu.Close();
                _waitEndCoroutione = StartCoroutine(WaitFadeCoroutine(onFadeEnd));
            }
        }

        private IEnumerator WaitFadeCoroutine(Action onFadeEnd = null)
        {
            float count = 0;
            _onFadeEndCallback = onFadeEnd;
            while(count < SceneTransition.Instance.Menu.TransitionDuration)
            {
                count += Time.deltaTime;
                yield return null;
            }
            FadeEnd();
            _waitEndCoroutione = null;
        }

        private void FadeEnd()
        {
            _isFading = false;
            SceneTransition.Instance.SetAnimationCurve(_previousCurve);
            _onFadeEndCallback?.Invoke();
            _onFadeEnd?.Invoke();
            _onFadeEndCallback = null;
        }

        private void OnDisable()
        {
            if(_waitEndCoroutione != null)
            {
                _waitEndCoroutione = null;
                FadeEnd();
            }
        }
    }
}