using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ivayami.UI {
    [RequireComponent(typeof(Menu))]
    public class PagePreset : MonoBehaviour {

        [Header("References")]

        [SerializeField] private TextMeshProUGUI[] _textBoxes;
        [SerializeField] private Image[] _images;

        public int TextBoxAmount => _textBoxes.Length;
        public int ImageAmount => _images.Length;

        public void DisplayContent(string[] texts, Sprite[] images) {
            for (int i = 0; i < _textBoxes.Length; i++) _textBoxes[i].text = texts[i];
            for (int i = 0; i < _images.Length; i++) {
                _images[i].enabled = images[i] != null;
                if(_images[i].enabled) _images[i].sprite = images[i];
            }
        }

    }
}
