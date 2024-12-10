using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
            _menuGroup.CloseCurrent();

            for (int i = 0; i < _entryImages.Length; i++) {
                Debug.Log($"{i} < {entry.Images.Length}");
                if (i < entry.Images.Length) _entryImages[i].sprite = entry.Images[i];
                else {
                    Debug.LogWarning("Entry doesn't fill every image slot in template");
                    break;
                }
            }

            StartCoroutine(FitEntryText(entry.GetDisplayDescription()));
        }

        private IEnumerator FitEntryText(string text) {
            foreach (TextMeshProUGUI entryNote in _entryNotes) {
                entryNote.text = text;

                yield return null;

                if (!entryNote.isTextOverflowing) {
                    _menuGroup.CloseCurrentThenOpen(_menuSelf);
                    yield break;
                }
                text = text.Substring(entryNote.firstOverflowCharacterIndex); // Doesn't update until frame passes
                entryNote.text = entryNote.text.Substring(0, entryNote.firstOverflowCharacterIndex);
            }
            _menuGroup.CloseCurrentThenOpen(_menuSelf);
            if (text.Length > 0) Debug.LogWarning($"Note couldn't fit entire text from scriptable object. Reduce total text or change entry preset to fit properly.");
        }

    }
}
