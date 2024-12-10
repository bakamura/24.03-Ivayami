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
            return LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Name");
        }

        public string GetDisplayDescription()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description");
        }
    }
}
