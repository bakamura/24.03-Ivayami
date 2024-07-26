using System;
using UnityEngine;
using TMPro;
using Ivayami.Save;
using Ivayami.UI;

public class UiLanguage : MonoBehaviour {

    [System.Serializable]
    public class LanguageComponent {
        [field: SerializeField] public TextMeshProUGUI Tmp { get; private set; }
        public string Id;
    }

    [Header("UI")]

    [SerializeField] private UiText _uiText;
    [SerializeField] private LanguageComponent[] _languageComponents;

    private void Start() {
        Options.OnChangeLanguage.AddListener(UpdateLanguage);
        UpdateLanguage((LanguageTypes) SaveSystem.Instance.Options.language);
    }

    public void UpdateLanguage(LanguageTypes language) {
        UiText uiText = _uiText.GetTranslation(language);
        foreach(LanguageComponent languageComponent in _languageComponents) languageComponent.Tmp.text = uiText.GetText(languageComponent.Id);
    }

    [ContextMenu("Set To UI Text")]
    private void SetToUiText() {
        if (_uiText != null) {
            if (_languageComponents == null) _languageComponents = new LanguageComponent[_uiText.Size];
            else Array.Resize(ref _languageComponents, _uiText.Size);
            for (int i = 0; i < _uiText.Size; i++) {
                if (_languageComponents[i] == null) _languageComponents[i] = new LanguageComponent();
                _languageComponents[i].Id = _uiText.Keys[i];
            }
        }
    }

}
