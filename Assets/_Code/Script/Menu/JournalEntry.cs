using UnityEngine;
using UnityEngine.Localization.Settings;
using Ivayami.Save;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Ivayami/UI/JournalyEntry")]
    public class JournalEntry : ScriptableObject {

#if UNITY_EDITOR
        public EntryContent[] DisplayTexts;

        [System.Serializable]
        public struct EntryContent
        {
            [ReadOnly] public string Language;
            public string Name;
            public string[] Descriptions;
        }
#endif
        public enum EntryCategory {
            Story,
            Character,
            Aberration
        }

        [field: SerializeField] public int TemplateID { get; private set; }
        [field: SerializeField] public EntryCategory Category { get; private set; }
        [SerializeField] private AreaProgress _progressType;
        [field: SerializeField] public Sprite[] Images { get; private set; }

        public string DisplayName()
        {
            return $"{name}/Name";
        }

        public string DisplayDescription()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Journal", $"{name}/Description_{(_progressType.Id != "StoryEntryProgress" ? SaveSystem.Instance.Progress.GetEntryProgressOfType(_progressType.Id) - 1 : 0)}");
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