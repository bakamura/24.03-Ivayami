using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.UI {
    public class Journal : MonoBehaviour {

        [SerializeField] private JournalDisplayPreset[] _presets;
        [SerializeField] private Button _selectionBtnPrefab;
        [SerializeField] private Animator _containerAnimator;

        [SerializeField] private RectTransform _storySelectionContainer;
        [SerializeField] private RectTransform _characterSelectionContainer;
        [SerializeField] private RectTransform _documentSelectionContainer;
        [SerializeField] private RectTransform _aberrationSelectionContainer;

        private static int _containerChange = Animator.StringToHash("Forward");

        public void ChangeAnimation() {
            _containerAnimator.SetTrigger(_containerChange);
        }

        public void SetupSelections() {
            // Needs to reset when entering a new game
            SetupStorySelection();
            SetupCharactersSelection();
            SetupDocumentsSelection();
            SetupAberrationsSelection();

            Resources.UnloadUnusedAssets();
        }

        private void SetupStorySelection() {
            for (int i = 0; i < SaveSystem.Instance.Progress.GetEntryProgressOfType("StoryEntry"); i++) if (i >= _storySelectionContainer.childCount)
                    SetupBtn(Instantiate(_selectionBtnPrefab, _storySelectionContainer), Resources.Load<JournalEntry>($"Journal/StoryEntry/ENUS/StoryEntry_{i}").GetTranslation(SaveSystem.Instance.Options.Language));
        }

        private void SetupCharactersSelection() {
            JournalEntry[] entries = Resources.LoadAll<JournalEntry>($"Journal/CharacterEntry/ENUS");
            int currentChild = 0;
            for (int i = 0; i < entries.Length; i++) if (SaveSystem.Instance.Progress.GetEntryProgressOfType(entries[i].name) > 0) {
                    SetupBtn(currentChild >= _documentSelectionContainer.childCount ? Instantiate(_selectionBtnPrefab, _documentSelectionContainer) : _documentSelectionContainer.GetChild(currentChild).GetComponentInChildren<Button>(), entries[i]);
                    currentChild++;
                }
        }

        private void SetupDocumentsSelection() {
            ReadableItem[] documentItems = PlayerInventory.Instance.CheckInventory().OfType<ReadableItem>().ToArray();
            for (int i = 0; i < documentItems.Length; i++)
                SetupBtn(i >= _documentSelectionContainer.childCount ? Instantiate(_selectionBtnPrefab, _documentSelectionContainer) : _documentSelectionContainer.GetChild(i).GetComponentInChildren<Button>(), documentItems[i].JournalEntry);
        }

        private void SetupAberrationsSelection() {
            JournalEntry[] entries = Resources.LoadAll<JournalEntry>($"Journal/AberrationEntry/ENUS");
            int currentChild = 0;
            for (int i = 0; i < entries.Length; i++) if (SaveSystem.Instance.Progress.GetEntryProgressOfType(entries[i].name) > 0) {
                    SetupBtn(currentChild >= _documentSelectionContainer.childCount ? Instantiate(_selectionBtnPrefab, _documentSelectionContainer) : _documentSelectionContainer.GetChild(currentChild).GetComponentInChildren<Button>(), entries[i]);
                    currentChild++;
                }
        }

        private void SetupBtn(Button btn, JournalEntry entry) {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => DisplayEntry(entry));
            btn.GetComponent<TextMeshProUGUI>().text = entry.DisplayName;
        }

        public void FocusFirstChapter(int chapterId) {
            _storySelectionContainer.GetChild(0).GetComponent<Button>().onClick.Invoke();

            Logger.Log(LogType.UI, $"Journal - Focus Chapter {chapterId}");
        }

        public void FocusFirstCharacter(int characterId) {
            _characterSelectionContainer.GetChild(0).GetComponent<Button>().onClick.Invoke();

            Logger.Log(LogType.UI, $"Journal - Focus Character {characterId}");
        }

        public void FocusFirstDocument() {
            Transform firstBtn = _documentSelectionContainer.GetChild(0);
            if (firstBtn != null) firstBtn.GetComponent<Button>().onClick.Invoke();
            else; // Display "No Entries
        }

        public void FocusFirstAberration(int aberrationId) {
            Transform firstBtn = _aberrationSelectionContainer.GetChild(0);
            if (firstBtn != null) firstBtn.GetComponent<Button>().onClick.Invoke();
            else; // Display "No Entries

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