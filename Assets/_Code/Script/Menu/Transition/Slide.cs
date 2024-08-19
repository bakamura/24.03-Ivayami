using System.Collections;
using UnityEngine;

namespace Ivayami.UI {
    public class Slide : Menu {

        [Header("Parameters")]

        [SerializeField] private Vector2 _openAnchoredPos;
        [SerializeField] private Vector2 _closedAnchoredPos;

        [Header("Cache")]

        private RectTransform _rectTransform;

        protected override void Awake() {
            base.Awake();

            _rectTransform = GetComponent<RectTransform>();
        }

        public override void Open() {
            StartCoroutine(Transition(true));

            Logger.Log(LogType.UI, $"Open Menu '{name}'");
        }

        public override void Close() {
            StartCoroutine(Transition(false));

            Logger.Log(LogType.UI, $"Close Menu '{name}'");
        }

        private IEnumerator Transition(bool isOpening) {
            if (!isOpening) {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            Vector2 initialPos = isOpening ? _closedAnchoredPos : _openAnchoredPos;
            Vector2 finalPos = isOpening ? _openAnchoredPos : _closedAnchoredPos;
            float currentDuration = 0f;
            while (currentDuration < 1f) {
                currentDuration += Time.deltaTime / TransitionDuration;
                _rectTransform.anchoredPosition = Vector2.Lerp(initialPos, finalPos, currentDuration);

                yield return null;
            }

            if (isOpening) {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

    }
}