using Ivayami.Save;
using UnityEngine;
using UnityEngine.Localization.Settings;
//using Ivayami.Puzzle;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Texts/JournalyEntry")]
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

        public string GetDisplayName()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Journal", $"{name}/Name");
        }

        public string GetDisplayDescription()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Journal", $"{name}/Description_{(_progressType != null ? SaveSystem.Instance.Progress.GetEntryProgressOfType(_progressType.Id) : 0)}");
        }

        //public JournalEntry(Readable readable) {
        //    DisplayTexts = new EntryContent[readable.DisplayTexts.Length];
        //    for(int i = 0; i < readable.DisplayTexts.Length; i++)
        //    {
        //        DisplayTexts[i].Name = readable.DisplayTexts[i].Name;
        //        DisplayTexts[i].Descriptions = new string[1];
        //        DisplayTexts[i].Descriptions[0] = readable.DisplayTexts[i].Description;
        //    }
        //    Images = new Sprite[0];
        //}

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

        //[ContextMenu("FIX")]
        //private void FixRef()
        //{
        //    JournalEntry[] assetsEN = Resources.LoadAll<JournalEntry>("Journal/StoryEntry/ENUS");
        //    JournalEntry[] assetsPTBR = Resources.LoadAll<JournalEntry>("Journal/StoryEntry/PTBR");
        //    for (int i = 0; i < assetsEN.Length; i++)
        //    {
        //        assetsEN[i].DisplayTexts = new EntryContent[LocalizationSettings.AvailableLocales.Locales.Count];
        //        assetsEN[i].DisplayTexts[0].Name = assetsEN[i].DisplayName;
        //        assetsEN[i].DisplayTexts[0].Descriptions = new string[assetsEN[i]._text.Length];
        //        for (int a = 0; a < assetsEN[i]._text.Length; a++)
        //        {
        //            assetsEN[i].DisplayTexts[a].Language = LocalizationSettings.AvailableLocales.Locales[a].LocaleName;
        //            assetsEN[i].DisplayTexts[0].Descriptions[a] = assetsEN[i]._text[a];
        //        }
        //    }
        //    for (int i = 0; i < assetsEN.Length; i++)
        //    {
        //        assetsEN[i].DisplayTexts[1].Name = assetsPTBR[i].DisplayName;
        //        assetsEN[i].DisplayTexts[1].Descriptions = new string[assetsPTBR[i]._text.Length];
        //        for (int a = 0; a < assetsEN[i]._text.Length; a++)
        //        {
        //            assetsEN[i].DisplayTexts[1].Descriptions[a] = assetsPTBR[i]._text[a];
        //        }
        //        UnityEditor.EditorUtility.SetDirty(assetsEN[i]);
        //    }
        //}
    }
}