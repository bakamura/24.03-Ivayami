using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ivayami.UI {
    [RequireComponent(typeof(Menu))]
    public class PagePreset : MonoBehaviour {

        [Header("References")]

        private TextMeshProUGUI[] _textBoxes;
        private Image[] _images;

        public int TextBoxAmount => _textBoxes.Length;
        public int ImageAmount => _images.Length;

        public void DisplayContent(string[] texts, Sprite[] images) {
            for (int i = 0; i < _textBoxes.Length; i++) _textBoxes[i].text = texts[i];
            for (int i = 0; i < _images.Length; i++) {
                _images[i].enabled = images[i] != null;
                if(_images[i].enabled) _images[i].sprite = images[i];
            }
        }

        //private void FitEntryText(string text) {
        //    foreach (TextMeshProUGUI content in PageTextBoxes) {
        //        content.text = text;
        //        content.ForceMeshUpdate();
        //        if (!content.isTextOverflowing) break;

        //        text = text.Substring(content.firstOverflowCharacterIndex); // Doesn't update until frame passes
        //        content.text = content.text.Substring(0, content.firstOverflowCharacterIndex);
        //    }
        //    if (text.Length > 0) Debug.LogWarning($"Note couldn't fit entire text from scriptable object. Reduce total text or change entry preset to fit properly.");
        //}

    }
}
