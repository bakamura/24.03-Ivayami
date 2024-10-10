using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ivayami.UI {
    public class JournalDisplayPreset : MonoBehaviour {

        [Header("References")]

        [SerializeField] private Image[] _entryImages;
        [SerializeField] private TextMeshProUGUI[] _entryNotes;

        public void DisplayEntry(JournalEntry entry) {
            for (int i = 0; i < _entryImages.Length; i++) {
                if (i < entry.Images.Length) _entryImages[i].sprite = entry.Images[i];
                else {
                    Debug.LogWarning("Entry doesn't fill every image slot in template");
                    break;
                }
            }
            int currentIndex = 0;
            string previousText;
            foreach (string word in entry.Text.Split(' ')) {
                previousText = _entryNotes[currentIndex].text;
                _entryNotes[currentIndex].text += $"{word} ";

                if (_entryNotes[currentIndex].isTextOverflowing) {
                    _entryNotes[currentIndex].text = previousText;
                    currentIndex++;
                    if (currentIndex < _entryNotes.Length) _entryNotes[currentIndex].text += $"{word} ";
                    else {
                        Debug.LogWarning("Entry text is too big for template");
                        break;
                    }
                }
            }
        }

    }
}
