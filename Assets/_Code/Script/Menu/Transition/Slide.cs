using System.Collections;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class Slide : Menu {

        [Header("Parameters")]

        [SerializeField] private Vector2 _openAnchoredPos;
        [SerializeField] private Vector2 _closedAnchoredPos;

        [Header("Cache")]

        private RectTransform _rectTransform;

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
        }

        public override void Open() {
            StartCoroutine(Transition(true));
        }

        public override void Close() {
            StartCoroutine(Transition(false));
        }

        private IEnumerator Transition(bool isOpening) {
            Vector2 initialPos = isOpening ? _closedAnchoredPos : _openAnchoredPos;
            Vector2 finalPos = isOpening ? _openAnchoredPos : _closedAnchoredPos;
            float currentDuration = 0f;
            while (currentDuration < 1f) {
                currentDuration += Time.deltaTime / _transitionDuration;
                _rectTransform.anchoredPosition = Vector2.Lerp(initialPos, finalPos, currentDuration);

                yield return null;
            }
        }

    }
}