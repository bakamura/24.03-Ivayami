using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using UnityEngine.Serialization; // Remove later

namespace Ivayami.UI {
    public class ScreenFade : MonoBehaviour {

        [Header("Events")]

        [SerializeField, Min(0f), FormerlySerializedAs("_delayBeforeStartFade")] private float _delayToFadeOut;
        [SerializeField, FormerlySerializedAs("_onAfterDelayBeforeFadeStart")] private UnityEvent _beforeFade;
        [SerializeField, FormerlySerializedAs("_onFadeEnd")] private UnityEvent _afterFade;

        [SerializeField, Min(0f)] private float _duration = 1f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Cache")]

        private static bool _isFading;
        private Action _onFadeEndCallback;
        private Coroutine _delayFadeStartCoroutine;
        private bool _isFadeIn;
        private bool _callbackExecuted;
        private const string BLOCK_KEY = "ScreenFade";

        [ContextMenu("FadeIn")]
        public void FadeIn() {
            if (!_isFading) {
                Debug.Log($"Fade In Request From {gameObject.name}");
                _isFading = true;
                _isFadeIn = true;
                _delayFadeStartCoroutine = StartCoroutine(FadeStartDelayCoroutine());
                PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false);
            }
        }

        [ContextMenu("FadeOut")]
        public void FadeOut() {
            if (!_isFading) {
                Debug.Log($"Fade Out Request From {gameObject.name}");
                _isFading = true;
                _isFadeIn = false;
                _delayFadeStartCoroutine = StartCoroutine(FadeStartDelayCoroutine());
                PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, true);
            }
        }

        private IEnumerator FadeStartDelayCoroutine() {
            if (!_isFadeIn) yield return new WaitForSeconds(_delayToFadeOut);
            _delayFadeStartCoroutine = null;
            if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
            SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
            _beforeFade?.Invoke();
            if (_isFadeIn) {
                SceneTransition.Instance.OnOpenEnd.AddListener(FadeEnd);
                SceneTransition.Instance.Open();
            }
            else {
                SceneTransition.Instance.OnCloseEnd.AddListener(FadeEnd);
                SceneTransition.Instance.Close();
            }
        }

        private void FadeEnd() {
            Debug.Log($"Fade End From {gameObject.name}");
            _callbackExecuted = true;
            _onFadeEndCallback?.Invoke();
            _afterFade?.Invoke();
            _onFadeEndCallback = null;
            _isFading = false;
            if (_isFadeIn) SceneTransition.Instance.OnOpenEnd.RemoveListener(FadeEnd);
            else SceneTransition.Instance.OnCloseEnd.RemoveListener(FadeEnd);
        }

        private void OnDisable() {
            if (!_callbackExecuted) FadeEnd();
            if (_delayFadeStartCoroutine != null) {
                _delayFadeStartCoroutine = null;
                _beforeFade?.Invoke();
                _isFading = false;
            }
        }
    }
}