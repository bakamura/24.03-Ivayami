using Ivayami.UI;
using System;
using TMPro;
using UnityEngine;

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
    }

    public void UpdateLanguage(LanguageTypes language) {
        UiText uiText = language == LanguageTypes.ENUS ? _uiText : Resources.Load<UiText>($"UiText/{language}/{_uiText.name}");
        foreach(LanguageComponent languageComponent in _languageComponents) languageComponent.Tmp.text = uiText.GetText(languageComponent.Id);
    }

    private void OnValidate() {
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
