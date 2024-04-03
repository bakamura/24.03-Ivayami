using System.Collections;
using UnityEngine;

namespace Paranapiacaba.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class Fade : Menu {

        [Header("Cache")]

        private CanvasGroup _canvasGroup;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void Open() {
            StartCoroutine(OpenRoutine());
        }

        public override void Close() {
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator OpenRoutine() {
            float currentDuration = 0f;
            while (currentDuration < 1f) {
                currentDuration += Time.deltaTime / _transitionDuration;
                _canvasGroup.alpha = _transitionCurve.Evaluate(currentDuration);

                yield return null;
            }
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private IEnumerator CloseRoutine() {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            float currentDuration = 1f;
            while (currentDuration > 0f) {
                currentDuration -= Time.deltaTime / _transitionDuration;
                _canvasGroup.alpha = _transitionCurve.Evaluate(currentDuration);

                yield return null;
            }
        }

    }
}