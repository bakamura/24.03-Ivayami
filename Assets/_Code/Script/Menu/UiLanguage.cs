using System;
using UnityEngine;
using TMPro;
using Ivayami.Save;

namespace Ivayami.UI {
    public class UiLanguage : MonoBehaviour {
        [Serializable]
        public class LanguageComponent {
            [field: SerializeField] public TextMeshProUGUI Tmp { get; private set; }
            public string Id;
        }

        [Header("UI")]

        [SerializeField] private UiText _uiText;
        [SerializeField] private LanguageComponent[] _languageComponents;

        private void Start() {
            Options.OnChangeLanguage.AddListener(UpdateLanguage);
            if (SaveSystem.Instance && SaveSystem.Instance.Options != null) UpdateLanguage((LanguageTypes)SaveSystem.Instance.Options.language);
        }

        private void OnDestroy() {
            Options.OnChangeLanguage.RemoveListener(UpdateLanguage);
        }

        public void UpdateLanguage(LanguageTypes language) {
            UiText uiText = _uiText.GetTranslation(language);
            foreach (LanguageComponent languageComponent in _languageComponents) languageComponent.Tmp.text = uiText.GetText(languageComponent.Id);
        }

        [ContextMenu("Set To UI Text")]
        private void SetToUiText() {
            if (_uiText != null) {
                if (_languageComponents == null) _languageComponents = new LanguageComponent[_uiText.Keys.Length];
                else Array.Resize(ref _languageComponents, _uiText.Keys.Length);
                for (int i = 0; i < _uiText.Keys.Length; i++) {
                    if (_languageComponents[i] == null) _languageComponents[i] = new LanguageComponent();
                    _languageComponents[i].Id = _uiText.Keys[i];
                }
            }
        }

    }
}
