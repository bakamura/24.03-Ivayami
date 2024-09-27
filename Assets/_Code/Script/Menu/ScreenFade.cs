using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization; // Remove later

namespace Ivayami.UI {
    public class ScreenFade : MonoBehaviour {

        [Header("Events")]

        [SerializeField, Min(0f), FormerlySerializedAs("_delayBeforeStartFade")] private float _delayToFadeOut;
        [SerializeField, FormerlySerializedAs("_onAfterDelayBeforeFadeStart")] private UnityEvent _beforeFade;
        [SerializeField, FormerlySerializedAs("_onFadeEnd")] private UnityEvent _afterFade;

        [SerializeField, Min(0f)] private float _duration = 1f;

        [Header("Cache")]

        private bool? _isFadeIn = null;
        private Coroutine _fadeOutDelayCoroutine;

        [ContextMenu("FadeIn")]
        public void FadeIn() {
            if (_isFadeIn == null) {
                _isFadeIn = true;

                if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);

                _beforeFade?.Invoke();
                SceneTransition.Instance.OnOpenEnd.AddListener(FadeEnd);
                SceneTransition.Instance.Open();
            }
        }

        [ContextMenu("FadeOut")]
        public void FadeOut() {
            if (_isFadeIn == null) {
                _isFadeIn = false;
                _fadeOutDelayCoroutine = StartCoroutine(FadeOutDelayRoutine());
            }
        }

        private IEnumerator FadeOutDelayRoutine() {
            yield return new WaitForSeconds(_delayToFadeOut);

            if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);

            _beforeFade?.Invoke();
            SceneTransition.Instance.OnCloseEnd.AddListener(FadeEnd);
            SceneTransition.Instance.Close();
            _fadeOutDelayCoroutine = null;
        }

        private void FadeEnd() {
            if (_isFadeIn.HasValue) {
                if (_isFadeIn.Value) SceneTransition.Instance.OnOpenEnd.RemoveListener(FadeEnd);
                else SceneTransition.Instance.OnCloseEnd.RemoveListener(FadeEnd);
                _isFadeIn = null;

                _afterFade?.Invoke();
            }
        }

        private void OnDisable() {
            if (_fadeOutDelayCoroutine != null) {
                _fadeOutDelayCoroutine = null;
                _beforeFade?.Invoke();
            }
            if (_isFadeIn != null) FadeEnd();
        }
    }
}