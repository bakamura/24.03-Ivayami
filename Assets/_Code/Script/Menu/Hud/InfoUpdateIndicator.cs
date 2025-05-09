using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

namespace Ivayami.UI {
    public class InfoUpdateIndicator : MonoSingleton<InfoUpdateIndicator> {

        [Header("Parameters")]

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _container;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;

        [Space(16)]

        [SerializeField] private float _showDuration;
        [SerializeField] private float _stayDuration;
        [SerializeField] private float _hideDuration;

        [Header("?")] // These should probably be external

        [SerializeField] private DisplayInfo _mapInfo;
        [SerializeField] private DisplayInfo _readableInfo;
        [SerializeField] private DisplayInfo _saveInfo;

        [Header("Cache")]

        private float _containerBaseWidth;
        private Queue<DisplayInfo> _displayQueue = new Queue<DisplayInfo>();
        private Coroutine _currentDisplayRoutine;
        private WaitForSeconds _stayWait;

        [System.Serializable]
        private class DisplayInfo {
            [field: SerializeField] public Sprite Sprite { get; private set; }
            [field: SerializeField] public LocalizedString LocalizedText { get; private set; }
            private string _text;
            public string Text { get { return LocalizedText != null ? LocalizedText.GetLocalizedString() : _text; } }

            public DisplayInfo(Sprite sprite, string text) {
                Sprite = sprite;
                _text = text;
            }
        }

        protected override void Awake() {
            base.Awake();

            _stayWait = new WaitForSeconds(_stayDuration);
            _containerBaseWidth = _container.sizeDelta.x;
        }

        public void DisplayUpdate(Sprite spriteToDisplay, string localizationId) {
            DisplayUpdate(new DisplayInfo(spriteToDisplay, localizationId));
        }

        private void DisplayUpdate(DisplayInfo infoToDisplay) {
            if (_currentDisplayRoutine != null) _displayQueue.Enqueue(infoToDisplay);
            else _currentDisplayRoutine = StartCoroutine(DisplayUpdateRoutine(infoToDisplay));
            Logger.Log(LogType.UI, $"Display Update Enqueue '{infoToDisplay.LocalizedText}' / {infoToDisplay.Sprite}");
        }

        private IEnumerator DisplayUpdateRoutine(DisplayInfo infoToDisplay) {
            _image.sprite = infoToDisplay.Sprite;
            _text.text = infoToDisplay.Text;
            _container.sizeDelta = new Vector2(_containerBaseWidth + _text.preferredWidth, _container.sizeDelta.y);

            while (_canvasGroup.alpha < 1f) {
                _canvasGroup.alpha += Time.deltaTime / _showDuration;

                yield return null;
            }

            yield return _stayWait;

            while (_canvasGroup.alpha > 0f) {
                _canvasGroup.alpha -= Time.deltaTime / _hideDuration;

                yield return null;
            }

            _currentDisplayRoutine = _displayQueue.Count > 0 ? StartCoroutine(DisplayUpdateRoutine(_displayQueue.Dequeue())) : null;
        }

        public void DisplayMap() {
            DisplayUpdate(_mapInfo);
        }

        public void DisplayReadable() {
            DisplayUpdate(_readableInfo);
        }

        public void DisplaySaved() {
            DisplayUpdate(_saveInfo);
        }

    }
}
