using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Ivayami.Localization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Ivayami/UI/Readable")]
    public class Readable : ScriptableObject {

        [field: SerializeField] public PagePreset[] PagePresets { get; private set; }
        [SerializeField] private PageContent[] _pagesContent;
        [Serializable]
        private struct PageContent {
            // Text Array
            public Sprite[] displayImages;
        }

        public string DisplayName => $"{name}/Name";
        public string[] Content => LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description").Split(SPLIT_CHAR);
        public Sprite[] GetPageSprites(int pageId) => _pagesContent[pageId].displayImages;

        private const char SPLIT_CHAR = '@';

#if UNITY_EDITOR
        public TextContent[] DisplayTexts;

        private void OnValidate() {
            if (PagePresets == null || PagePresets.Length == 0) PagePresets = new PagePreset[1];
            for (int i = 0; i < PagePresets.Length; i++) {
                if (PagePresets[i] == null) PagePresets[i] = AssetDatabase.LoadAssetAtPath<PagePreset>("Assets/_Game/Prefab/Menu/PagePresets/PagePreset_0.prefab");
            }
            ResizeToPresets();
            ResizeText();
        }

        private void ResizeToPresets() {
            if (_pagesContent == null) _pagesContent = new PageContent[1];
            if (_pagesContent.Length != PagePresets.Length) {
                Array.Resize(ref _pagesContent, PagePresets.Length);
                for (int i = 0; i < _pagesContent.Length; i++) {
                    if (!PagePresets[i]) continue;
                    //if (_pagesContent[i].textBoxes.Length != PagePresets[i].TextBoxAmount) Array.Resize(ref _pagesContent[i].textBoxes, PagePresets[i].TextBoxAmount);
                    if (_pagesContent[i].displayImages.Length != PagePresets[i].ImageAmount) Array.Resize(ref _pagesContent[i].displayImages, PagePresets[i].ImageAmount);
                }
            }
        }

        private void ResizeText() {
            if (DisplayTexts == null || DisplayTexts.Length == 0) return;
            int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;
            if (languagesCount > 0 && DisplayTexts.Length != languagesCount) {
                Array.Resize(ref DisplayTexts, languagesCount);
                for (int i = 0; i < DisplayTexts.Length; i++) {
                    DisplayTexts[i].Language = LocalizationSettings.AvailableLocales.Locales[i].LocaleName;
                }
            }
        }
#endif

    }
}
