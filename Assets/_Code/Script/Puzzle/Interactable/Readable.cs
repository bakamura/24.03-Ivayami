using UnityEngine;
using UnityEngine.Localization.Settings;
using Ivayami.Localization;

namespace Ivayami.Puzzle {
    [CreateAssetMenu(menuName = "Ivayami/UI/Readable")]
    public class Readable : ScriptableObject {

        [field: SerializeField] public Sprite[] DisplayImages;

        public string DisplayName => $"{name}/Name";
        public string DisplayContent => LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description");

#if UNITY_EDITOR
        public TextContent[] DisplayTexts;

        private void OnValidate()
        {
            if (DisplayTexts == null || DisplayTexts.Length == 0) return;
            int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;
            if (languagesCount > 0 && DisplayTexts.Length != languagesCount)
            {
                System.Array.Resize(ref DisplayTexts, languagesCount);
                for (int i = 0; i < DisplayTexts.Length; i++)
                {
                    DisplayTexts[i].Language = LocalizationSettings.AvailableLocales.Locales[i].LocaleName;
                }
            }
        }
#endif
    }
}
