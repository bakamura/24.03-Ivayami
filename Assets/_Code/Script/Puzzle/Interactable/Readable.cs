using UnityEngine;
using UnityEngine.Localization.Settings;
using Ivayami.Localization;

namespace Ivayami.Puzzle {
    [CreateAssetMenu(menuName = "Texts/Readable")]
    public class Readable : ScriptableObject {

#if UNITY_EDITOR
        public TextContent[] DisplayTexts;
#endif
        public string GetDisplayName()
        {
            return $"{name}/Name";
        }

        public string GetDisplayDescription()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description");
        }

#if UNITY_EDITOR
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
