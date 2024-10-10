using System.Linq;
using UnityEngine;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "ChapterInfo/Description")]
    public class JournalEntry : ScriptableObject {

        public enum EntryCategory {
            Story,
            Character,
            Aberration
        }

        [field: SerializeField] public int TemplateID { get; private set; }
        [field: SerializeField] public EntryCategory Category { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea] public string Text { get; private set; }
        [field: SerializeField] public Sprite[] Images { get; private set; }

        public JournalEntry(string displayName, string content) {
            DisplayName = displayName;
            Text = content;
        }

        public JournalEntry GetTranslation(LanguageTypes language) {
            if (language == LanguageTypes.ENUS) return this;
            JournalEntry entry = Resources.LoadAll<JournalEntry>($"JournalEntry/{language}/").First(text => text.name == name);
            Resources.UnloadUnusedAssets();
            if (entry != null) return entry;
            else {
                Debug.LogError($"No translation {language} found of '{name}' (JournalEntry)");
                return this;
            }
        }

    }
}