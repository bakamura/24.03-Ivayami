using UnityEngine;
using TMPro;
using Ivayami.Save;

namespace Ivayami.UI {
    public class UiLanguageDynamic : MonoBehaviour {

        [Header("References")]

        private TextMeshProUGUI _tmp;
        [SerializeField] private UiText _uiText;

        private void Awake() {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        public void DisplayText(string textId) {
            _tmp.text = _uiText.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language).GetText(textId);
        }

        public void DisplayText(string textId, UiText uiText) {
            _tmp.text = uiText.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language).GetText(textId);
        }

    }
}
