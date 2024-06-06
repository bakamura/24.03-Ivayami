using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class InfoUpdateIndicator : MonoBehaviour {
        
        [Header("Parameters")]

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _image;

        [Space(16)]

        [SerializeField] private float _showDuration;
        [SerializeField] private float _stayDuration;
        [SerializeField] private float _hideDuration;

        [Header("Debug")]

        [SerializeField] private Sprite _mapIcon;

        [Header("Cache")]

        private Queue<Sprite> _displayQueue = new Queue<Sprite>();
        private Coroutine _currentDisplayRoutine;
        private WaitForSeconds _stayWait;

        public void DisplayUpdate(Sprite spriteToDisplay) {
            if (_currentDisplayRoutine != null) _displayQueue.Enqueue(spriteToDisplay);
            Logger.Log(LogType.UI, $"Display Update {spriteToDisplay.name}");
            _currentDisplayRoutine = StartCoroutine(DisplayUpdateRoutine(spriteToDisplay));
        }

        private IEnumerator DisplayUpdateRoutine(Sprite spriteToDisplay) {
            _image.sprite = spriteToDisplay;

            while (_canvasGroup.alpha < 1f) {
                _canvasGroup.alpha += Time.deltaTime / _showDuration;

                yield return null;
            }

            yield return _stayWait;

            while (_canvasGroup.alpha > 0f) {
                _canvasGroup.alpha -= Time.deltaTime / _hideDuration;

                yield return null;
            }

            _currentDisplayRoutine = _displayQueue.Count > 0 ? StartCoroutine(DisplayUpdateRoutine(_displayQueue.Dequeue())) :  null;
        }

        public void DisplayMap() {
            DisplayUpdate(_mapIcon);
        }

    }
}
