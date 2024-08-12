using System.Collections;
using UnityEngine;

namespace Ivayami.UI {
    public class Fade : Menu {

        [SerializeField] private bool _interactable;

        public override void Open() {
            StartCoroutine(OpenRoutine());

            Logger.Log(LogType.UI, $"Open Menu '{name}'");
        }

        public override void Close() {
            StartCoroutine(CloseRoutine());

            Logger.Log(LogType.UI, $"Close Menu '{name}'");
        }

        private IEnumerator OpenRoutine() {
            float currentDuration = 0f;
            while (currentDuration < 1f) {
                currentDuration += Time.deltaTime / TransitionDuration;
                _canvasGroup.alpha = _transitionCurve.Evaluate(currentDuration);

                yield return null;
            }
            if (_interactable) {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        private IEnumerator CloseRoutine() {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            float currentDuration = 1f;
            while (currentDuration > 0f) {
                currentDuration -= Time.deltaTime / TransitionDuration;
                _canvasGroup.alpha = _transitionCurve.Evaluate(currentDuration);

                yield return null;
            }
        }

    }
}