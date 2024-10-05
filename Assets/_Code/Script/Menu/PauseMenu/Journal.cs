using System.Linq;
using UnityEngine;
using TMPro;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Journal : MonoBehaviour {

        [SerializeField] private JournalDisplayPreset[] _presets;
        [SerializeField] private RectTransform _selectionBtnPrefab;
        [SerializeField] private Animator _containerAnimator;

        [SerializeField] private RectTransform _storySelectionContainer;
        [SerializeField] private RectTransform _characterSelectionContainer;
        [SerializeField] private RectTransform _documentSelectionContainer;
        [SerializeField] private RectTransform _aberrationSelectionContainer;

        private static int _containerChange = Animator.StringToHash("Forward");

        private const string STORY_ENTRY = "ChapterDescription";
        private const string CHARACTER_ENTRY = "CharacterDescription";
        private const string DOCUMENT_ENTRY = "Document";
        private const string ABERRATION__ENTRY = "AberrationDescription";

        public void ChangeAnimation() {
            _containerAnimator.SetTrigger(_containerChange);
        }

        public void SetupSelections() {
            // Check Current Story Progress for Story
            // Check Current Story Progress for Characters
            InventoryItem[] documentItems = PlayerInventory.Instance.CheckInventory().Where(item => item.Type == ItemType.Document).ToArray();
            for (int i = 0; i < documentItems.Length; i++) {
                if (_documentSelectionContainer.childCount <= i) Instantiate(_selectionBtnPrefab, _documentSelectionContainer);
                _documentSelectionContainer.GetChild(i).GetComponentInChildren<TextMeshPro>().text = documentItems[i].DisplayName;
            }
            // Somehow Check Known Creatures
        }

        public void FocusChapter(int chapterId) {
            DisplayEntry(Resources.Load<JournalEntry>($"{STORY_ENTRY}/ChapterDescription_{chapterId}"));

            Logger.Log(LogType.UI, $"Journal - Focus Chapter {chapterId}");
        }

        public void FocusCharacter(int characterId) {
            DisplayEntry(Resources.Load<JournalEntry>($"{CHARACTER_ENTRY}/CharacterDescription_{(true ? characterId : "null")}"));

            Logger.Log(LogType.UI, $"Journal - Focus Character {characterId}");
        }

        public void FocusDocument(int documentId) {
            DisplayEntry(Resources.Load<JournalEntry>($"{DOCUMENT_ENTRY}/Document_{(true ? documentId : "null")}"));

            Logger.Log(LogType.UI, $"Journal - Focus Document {documentId}");
        }

        public void FocusAberration(int aberrationId) {
            DisplayEntry(Resources.Load<JournalEntry>($"{ABERRATION__ENTRY}/AberrationDescription_{(true ? aberrationId : "null")}"));

            Logger.Log(LogType.UI, $"Journal - Focus Aberration {aberrationId}");
        }

        private void DisplayEntry(JournalEntry entry) {
            if (entry == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            _presets[entry.TemplateID].DisplayEntry(entry);
        }

    }
}