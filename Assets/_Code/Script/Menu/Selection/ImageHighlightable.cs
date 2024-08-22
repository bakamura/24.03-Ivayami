using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class ImageHighlightable : Highlightable {

        [Header("Parameters")]

        [SerializeField] private Sprite _normalImage;
        [SerializeField] private Sprite _highlightImage;

        private Image _image;

        private void Awake() {
            _image = GetComponent<Image>();
        }

        public override void Highlight() {
            _image.sprite = _highlightImage;
        }

        public override void Conceal() {
            _image.sprite = _normalImage;
        }

    }
}
