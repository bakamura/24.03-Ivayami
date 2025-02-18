using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;
using Ivayami.Save;
using Ivayami.Audio;
using System.Collections;
using Ivayami.Puzzle;

namespace Ivayami.UI {
    public class Journal : MonoBehaviour {

        [SerializeField] private JournalDisplayPreset[] _presets;
        [SerializeField] private Button _selectionBtnPrefab;
        [SerializeField] private Animator _containerAnimator;

        [SerializeField] private UiSound _btnSound;
        [SerializeField] private Button _chapterBtn;
        [SerializeField] private RectTransform _storySelectionContainer;
        [SerializeField] private RectTransform _characterSelectionContainer;
        [SerializeField] private RectTransform _documentSelectionContainer;
        [SerializeField] private RectTransform _aberrationSelectionContainer;

        [SerializeField] private MenuGroup _displayMenuGroup;
        [SerializeField] private MenuGroup _noEntriesMenuGroup;
        [SerializeField] private Menu _noEntriesMenu;
        private bool _shouldResetToStory;

        private static int _containerChange = Animator.StringToHash("Forward");

        private void Start() {
            Menu menu = GetComponent<Menu>();
            menu.OnOpenStart.AddListener(() => {
                _shouldResetToStory = true;
            });
            menu.OnCloseStart.AddListener(() => {
                _shouldResetToStory = false;
            });
        }

        public void ChangeAnimation() {
            _containerAnimator.SetTrigger(_containerChange);
        }

        public void SetupSelections() {
            SetupStorySelection();
            SetupCharactersSelection();
            SetupDocumentsSelection();
            SetupAberrationsSelection();

            Resources.UnloadUnusedAssets();
            StartCoroutine(ResetToSToryCoroutine());
        }

        private IEnumerator ResetToSToryCoroutine() {
            yield return new WaitForEndOfFrame();
            if (_shouldResetToStory) _chapterBtn.onClick.Invoke();
        }

        public void ResetSelections() {
            int childCount = _storySelectionContainer.childCount;
            for (int i = 0; i < childCount; i++) Destroy(_storySelectionContainer.GetChild(0).gameObject);
            childCount = _characterSelectionContainer.childCount;
            for (int i = 0; i < childCount; i++) Destroy(_characterSelectionContainer.GetChild(0).gameObject);
            childCount = _documentSelectionContainer.childCount;
            for (int i = 0; i < childCount; i++) Destroy(_documentSelectionContainer.GetChild(0).gameObject);
            childCount = _aberrationSelectionContainer.childCount;
            for (int i = 0; i < childCount; i++) Destroy(_aberrationSelectionContainer.GetChild(0).gameObject);
        }

        private void SetupStorySelection() {
            int progress = SaveSystem.Instance.Progress.GetEntryProgressOfType("StoryEntryProgress") - 1;
            if (progress >= 0) for (int i = 0; i <= progress; i++) if (i >= _storySelectionContainer.childCount) SetupBtn(Instantiate(_selectionBtnPrefab, _storySelectionContainer), Resources.Load<JournalEntry>($"Journal/StoryEntry/StoryEntry_{i}"));
        }

        private void SetupCharactersSelection() {
            JournalEntry[] entries = Resources.LoadAll<JournalEntry>($"Journal/CharacterEntry");
            int currentChild = 0;
            for (int i = 0; i < entries.Length; i++) if (SaveSystem.Instance.Progress.GetEntryProgressOfType(entries[i].name) > 0) {
                    SetupBtn(currentChild >= _characterSelectionContainer.childCount ? Instantiate(_selectionBtnPrefab, _characterSelectionContainer) : _characterSelectionContainer.GetChild(currentChild).GetComponentInChildren<Button>(), entries[i]);
                    currentChild++;
                }
        }

        private void SetupDocumentsSelection() {
            ReadableItem[] documentItems = PlayerInventory.Instance.CheckInventory().OfType<ReadableItem>().ToArray();
            for (int i = 0; i < documentItems.Length; i++)
                SetupBtn(i >= _documentSelectionContainer.childCount ? Instantiate(_selectionBtnPrefab, _documentSelectionContainer) : _documentSelectionContainer.GetChild(i).GetComponentInChildren<Button>(), documentItems[i].Entry);
        }

        private void SetupAberrationsSelection() {
            JournalEntry[] entries = Resources.LoadAll<JournalEntry>($"Journal/AberrationEntry");
            int currentChild = 0;
            for (int i = 0; i < entries.Length; i++) if (SaveSystem.Instance.Progress.GetEntryProgressOfType(entries[i].name) > 0) {
                    SetupBtn(currentChild >= _aberrationSelectionContainer.childCount ? Instantiate(_selectionBtnPrefab, _aberrationSelectionContainer) : _aberrationSelectionContainer.GetChild(currentChild).GetComponentInChildren<Button>(), entries[i]);
                    currentChild++;
                }
        }

        private void SetupBtn(Button btn, JournalEntry entry, bool shouldSelect = false) {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => DisplayEntry(entry));
            btn.onClick.AddListener(_btnSound.GoForth);
            btn.GetComponent<TextMeshProUGUI>().text = entry.GetDisplayName();
            if (shouldSelect) btn.onClick.Invoke();
        }

        private void SetupBtn(Button btn, Readable entry, bool shouldSelect = false) {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => DisplayEntry(entry));
            btn.onClick.AddListener(_btnSound.GoForth);
            btn.GetComponent<TextMeshProUGUI>().text = entry.GetDisplayName();
            if (shouldSelect) btn.onClick.Invoke();
        }

        public void FocusFirstChapter() {
            GameObject btnGO = _storySelectionContainer.GetChild(0).gameObject;
            btnGO.GetComponent<Button>().onClick.Invoke();
            _displayMenuGroup.SetSelected(btnGO);
            _noEntriesMenuGroup.CloseCurrent();

            Logger.Log(LogType.UI, $"Journal - Focus First Chapter");
        }

        public void FocusFirstCharacter() {
            GameObject btnGO = _characterSelectionContainer.GetChild(0).gameObject;
            btnGO.GetComponent<Button>().onClick.Invoke();
            _displayMenuGroup.SetSelected(btnGO);
            _noEntriesMenuGroup.CloseCurrent();

            Logger.Log(LogType.UI, $"Journal - Focus First Character");
        }

        public void FocusFirstDocument() {
            if (_documentSelectionContainer.childCount > 0) {
                GameObject btnGO = _documentSelectionContainer.GetChild(0).gameObject;
                btnGO.GetComponent<Button>().onClick.Invoke();
                _displayMenuGroup.SetSelected(btnGO);
                _noEntriesMenuGroup.CloseCurrent();
            }
            else {
                _displayMenuGroup.CloseCurrent();
                _noEntriesMenuGroup.CloseCurrentThenOpen(_noEntriesMenu);
            }

            Logger.Log(LogType.UI, $"Journal - Focus First Document");
        }

        public void FocusFirstAberration() {
            if (_aberrationSelectionContainer.childCount > 0) {
                GameObject btnGO = _aberrationSelectionContainer.GetChild(0).gameObject;
                btnGO.GetComponent<Button>().onClick.Invoke();
                _displayMenuGroup.SetSelected(btnGO);
                _noEntriesMenuGroup.CloseCurrent();
            }
            else {
                _displayMenuGroup.CloseCurrent();
                _noEntriesMenuGroup.CloseCurrentThenOpen(_noEntriesMenu);
            }

            Logger.Log(LogType.UI, $"Journal - Focus First Aberration");
        }

        private void DisplayEntry(JournalEntry entry) {
            if (entry == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            if (_presets.Length > entry.TemplateID) _presets[entry.TemplateID].DisplayEntry(entry);
            else Debug.LogError($"'{entry.name}' tried using journal preset {entry.TemplateID} which doesn't exist!");
        }

        private void DisplayEntry(Readable entry) {
            if (entry == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            _presets[0].DisplayEntry(entry);
        }
    }
}