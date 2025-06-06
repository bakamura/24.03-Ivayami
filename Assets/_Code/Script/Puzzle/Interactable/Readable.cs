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
        }

        private void AssignEmptyPresets() {
            if (PagePresets == null || PagePresets.Length == 0) PagePresets = new PagePreset[1];
            for (int i = 0; i < PagePresets.Length; i++) if (PagePresets[i] == null)
                    PagePresets[i] = AssetDatabase.LoadAssetAtPath<PagePreset>("Assets/_Game/Prefab/Menu/PagePresets/PagePreset_0.prefab");
        }

        private void ResizeToPresets() {
            if (_pagesContent == null) _pagesContent = new PageContent[0];
            if (_pagesContent.Length != PagePresets.Length) Array.Resize(ref _pagesContent, PagePresets.Length);
            for (int i = 0; i < _pagesContent.Length; i++) {
                if (_pagesContent[i].textBoxes == null) _pagesContent[i].textBoxes = new PageTextBox[0];
                if (_pagesContent[i].textBoxes.Length != PagePresets[i].TextBoxAmount) Array.Resize(ref _pagesContent[i].textBoxes, PagePresets[i].TextBoxAmount); // Text Boxes
                for (int j = 0; j < _pagesContent[i].textBoxes.Length; j++) {
                    if (_pagesContent[i].textBoxes[j].textTranslations == null) _pagesContent[i].textBoxes[j].textTranslations = new TextTranslation[0];
                    _pagesContent[i].textBoxes[j].textTranslations = ResizeTextTranslations(_pagesContent[i].textBoxes[j].textTranslations); // Text Translations
                }
                if (_pagesContent[i].images == null) _pagesContent[i].images = new Sprite[0];
                if (_pagesContent[i].images.Length != PagePresets[i].ImageAmount) Array.Resize(ref _pagesContent[i].images, PagePresets[i].ImageAmount); // Images
            }
        }

        private TextTranslation[] ResizeTextTranslations(TextTranslation[] textTranslations) {
            int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;

            if (textTranslations == null || textTranslations.Length == 0) textTranslations = new TextTranslation[languagesCount];
            else if (languagesCount > 0 && textTranslations.Length != languagesCount) Array.Resize(ref textTranslations, languagesCount);
            else return textTranslations;

            for (int i = 0; i < textTranslations.Length; i++) textTranslations[i].language = LocalizationSettings.AvailableLocales.Locales[i].LocaleName;
            return textTranslations;
        }
#endif

    }
}
