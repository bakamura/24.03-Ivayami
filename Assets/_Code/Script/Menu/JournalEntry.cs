using Ivayami.Save;
using System.Linq;
using UnityEngine;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Texts/JournalyEntry")]
    public class JournalEntry : ScriptableObject {

        public enum EntryCategory {
            Story,
            Character,
            Aberration
        }

        [field: SerializeField] public int TemplateID { get; private set; }
        [field: SerializeField] public EntryCategory Category { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [SerializeField] private AreaProgress _progressType;

        [SerializeField, TextArea] private string[] _text;
        public string Text { get { return string.Join("\n", _progressType != null ? _text.Take(SaveSystem.Instance.Progress.GetEntryProgressOfType(_progressType.Id)).ToArray() : _text); } }
        [field: SerializeField] public Sprite[] Images { get; private set; }

        public JournalEntry(string displayName, string content) {
            DisplayName = displayName;
            _text = new string[] { content };
            Images = new Sprite[0];
        }

        public JournalEntry(string displayName, string[] content) {
            DisplayName = displayName;
            _text = content;
            Images = new Sprite[0];
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