using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using static Ivayami.UI.Readable.PageContent;


#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Ivayami/UI/Readable")]
    public class Readable : ScriptableObject {

#if UNITY_EDITOR
        [field: SerializeField] public TextTranslation[] TitleTranslations { get; private set; }
        [Serializable]
        public struct TextTranslation {
            [TextArea(1, 50)] public string text;
            [ReadOnly] public string language;
        }

        public string GetTextBoxesTranslated(int pageId, int language) => string.Join(SPLIT_CHAR, _pagesContent[pageId].textBoxes.Select(b => b.textTranslations[language].text));
#endif

        [field: SerializeField] public PagePreset[] PagePresets { get; private set; }
        [SerializeField] private PageContent[] _pagesContent;

        [Serializable]
        public struct PageContent {
#if UNITY_EDITOR
            public PageTextBox[] textBoxes;
            [Serializable]
            public struct PageTextBox {
                public TextTranslation[] textTranslations;
            }
#endif
            public Sprite[] images;
        }

        public string TitleLocalizationString => $"{name}/Name";
        public string[] GetTextBoxes(int pageId) => LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description_{pageId}").Split(SPLIT_CHAR);
        public Sprite[] GetPageSprites(int pageId) => _pagesContent[pageId].images;

        public const char SPLIT_CHAR = '@';

#if UNITY_EDITOR
        private void OnValidate() {
            AssignEmptyPresets();
            TitleTranslations = ResizeTextTranslations(TitleTranslations);
            ResizeToPresets();

#pragma warning disable CS0162 // Unreachable code detected
            if (false) CopyFromTables(); // Use / Adapt when making the inverse path (Tables => Asset, instead of Asset => Tables)
#pragma warning restore CS0162
        }

        private void AssignEmptyPresets() {
            if (PagePresets == null || PagePresets.Length == 0) PagePresets = new PagePreset[1];
            PagePreset defaultPreset = AssetDatabase.LoadAssetAtPath<PagePreset>("Assets/_Game/Prefab/Menu/PagePresets/PagePreset_0.prefab");
            for (int i = 0; i < PagePresets.Length; i++) if (PagePresets[i] == null) PagePresets[i] = defaultPreset;
        }

        private void ResizeToPresets() {
            if (_pagesContent == null) _pagesContent = new PageContent[0];
            if (_pagesContent.Length != PagePresets.Length) Array.Resize(ref _pagesContent, PagePresets.Length);
            for (int i = 0; i < _pagesContent.Length; i++) {
                _pagesContent[i].textBoxes ??= new PageTextBox[0];
                if (_pagesContent[i].textBoxes.Length != PagePresets[i].TextBoxAmount) Array.Resize(ref _pagesContent[i].textBoxes, PagePresets[i].TextBoxAmount); // Text Boxes
                for (int j = 0; j < _pagesContent[i].textBoxes.Length; j++) {
                    _pagesContent[i].textBoxes[j].textTranslations ??= new TextTranslation[0];
                    _pagesContent[i].textBoxes[j].textTranslations = ResizeTextTranslations(_pagesContent[i].textBoxes[j].textTranslations); // Text Translations
                }
                _pagesContent[i].images ??= new Sprite[0];
                if (_pagesContent[i].images.Length != PagePresets[i].ImageAmount) Array.Resize(ref _pagesContent[i].images, PagePresets[i].ImageAmount); // Images
            }
        }

        private TextTranslation[] ResizeTextTranslations(TextTranslation[] textTranslations) {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            int languagesCount = locales.Count;

            if (textTranslations == null || textTranslations.Length == 0) textTranslations = new TextTranslation[languagesCount];
            else if (languagesCount > 0 && textTranslations.Length != languagesCount) Array.Resize(ref textTranslations, languagesCount);
            else return textTranslations;

            for (int i = 0; i < textTranslations.Length; i++) textTranslations[i].language = locales[i].LocaleName;
            return textTranslations;
        }

        private void CopyFromTables() {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            for (int i = 0; i < locales.Count; i++) {
                TitleTranslations[i].text = LocalizationSettings.StringDatabase.GetLocalizedString("Items", TitleLocalizationString, locales[i]);
                _pagesContent[0].textBoxes[0].textTranslations[i].text = LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description", locales[i]);
            }
        }
#endif

    }
}
