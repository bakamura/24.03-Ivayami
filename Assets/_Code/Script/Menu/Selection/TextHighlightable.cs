
using UnityEngine;
using TMPro;

namespace Ivayami.UI {
    public class TextHighlightable : Highlightable {

        [Header("Parameters")]

        [SerializeField] private TMP_FontAsset _normalFontAsset;
        [SerializeField] private TMP_FontAsset _highlightFontAsset;

        private TextMeshProUGUI _tmp;

        private void Awake() {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        public override void Highlight() {
            _tmp.font = _highlightFontAsset;
        }

        public override void Conceal() {
            _tmp.font = _normalFontAsset;
        }

    }
}
