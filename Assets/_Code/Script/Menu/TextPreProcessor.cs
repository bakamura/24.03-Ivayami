using UnityEngine;
using TMPro;

namespace Ivayami.UI {
    public class TextPreProcessor : MonoBehaviour, ITextPreprocessor {

        [SerializeField] private TextPreProcessorSettings _settings;

        private void Awake() {
            GetComponent<TMP_Text>().textPreprocessor = this;
        }

        public string PreprocessText(string text) {
            string processedText = text;
            foreach (TextTag tag in _settings.TextTags) {
                processedText = processedText.Replace($"<{tag.name}>", $"<color=#{ColorUtility.ToHtmlStringRGB(tag.color)}>");
                processedText = processedText.Replace($"</{tag.name}>", $"</color>");
            }
            return processedText;
        }
    }

}
