using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ivayami.UI {
    [RequireComponent(typeof(Menu))]
    public class JournalDisplayPreset : MonoBehaviour {

        [Header("References")]

        [SerializeField] private Image[] _entryImages;
        [SerializeField] private TextMeshProUGUI[] _entryNotes;

        [Header("Cache")]

        private MenuGroup _menuGroup;
        private Menu _menuSelf;

        private void Awake() {
            _menuGroup = GetComponentInParent<MenuGroup>();
            _menuSelf = GetComponent<Menu>();
        }

        public void DisplayEntry(JournalEntry entry) {
            foreach (TextMeshProUGUI entryNote in _entryNotes) entryNote.text = string.Empty;
            _menuGroup.CloseCurrentThenOpen(_menuSelf);

            for (int i = 0; i < _entryImages.Length; i++) {
                Debug.Log($"{i} < {entry.Images.Length}");
                if (i < entry.Images.Length) _entryImages[i].sprite = entry.Images[i];
                else {
                    Debug.LogWarning("Entry doesn't fill every image slot in template");
                    break;
                }
            }
            //int currentIndex = 0;
            //string previousText;
            //foreach (string word in entry.Text.Split(' ')) {
            //    previousText = _entryNotes[currentIndex].text;
            //    _entryNotes[currentIndex].text += $"{word} ";

            //    if (_entryNotes[currentIndex].isTextOverflowing) {
            //        _entryNotes[currentIndex].text = previousText;
            //        currentIndex++;
            //        if (currentIndex < _entryNotes.Length) _entryNotes[currentIndex].text += $"{word} ";
            //        else {
            //            Debug.LogWarning($"Entry text is too big for template. Last word '{word}' (id: {currentIndex})");
            //            break;
            //        }
            //    }
            //}

            string text = entry.Text;
            foreach(TextMeshProUGUI entryNote in _entryNotes) {
                entryNote.text = text;
                text = text.Substring(entryNote.firstOverflowCharacterIndex); // Doesn't update until frame passes
                entryNote.text = entryNote.text.Substring(entryNote.firstOverflowCharacterIndex);
            }
        }

    }
}
