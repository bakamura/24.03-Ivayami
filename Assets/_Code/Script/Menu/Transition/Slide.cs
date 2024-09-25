using UnityEngine;

namespace Ivayami.UI {
    public class Slide : Menu {

        [Header("Slide")]

        [SerializeField] private Vector2 _openStartPos;
        [SerializeField] private Vector2 _openEndPos;
        [SerializeField] private Vector2 _closeStartPos;
        [SerializeField] private Vector2 _closeEndPos;

        [Header("Cache")]

        private RectTransform _rectTransform;

        protected override void Awake() {
            base.Awake();

            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void TransitionBehaviour(float currentPhase) {
            _rectTransform.anchoredPosition = Vector2.Lerp(_isOpening ? _openStartPos : _openEndPos, _isOpening ? _closeStartPos : _closeEndPos, _transitionCurve.Evaluate(currentPhase));
        }

    }
}