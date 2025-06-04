using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Ivayami.Localization;

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
            if (_pagesContent.Length != PagePresets.Length) {
                Array.Resize(ref _pagesContent, PagePresets.Length);
                for (int i = 0; i < _pagesContent.Length; i++) {
                    if (!PagePresets[i]) continue;
                    // Resize Text
                    //if (_pagesContent[i].displayImages.Length != PagePresets[i].PageImages.Length) Array.Resize(ref _pagesContent[i].displayImages, PagePresets[i].PageImages.Length);
                }
            }

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
