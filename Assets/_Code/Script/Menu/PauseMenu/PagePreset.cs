using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Ivayami.UI {
    [RequireComponent(typeof(Menu))]
    public class PagePreset : MonoBehaviour {

        [Header("References")]

        [field: SerializeField] public Image[] PageImages;
        [field: SerializeField] public TextMeshProUGUI[] PageContents;

        // Will be removed
        public void DisplayEntry(JournalEntry entry) {
            foreach (TextMeshProUGUI content in PageContents) content.text = string.Empty;

            for (int i = 0; i < PageImages.Length; i++) {
                Debug.Log($"{i} < {entry.Images.Length}");
                if (i < entry.Images.Length) PageImages[i].sprite = entry.Images[i];
                else {
                    Debug.LogWarning("Entry doesn't fill every image slot in template");
                    break;
                }
            }
            FitEntryText(entry.DisplayDescription());
        }

        public void DisplayEntry(Readable entry) {
            //foreach (TextMeshProUGUI content in PageContents) content.text = string.Empty;
            FitEntryText(entry.DisplayContent);
        }

        // Chanbge to prefer to keep each text within its own box
        private void FitEntryText(string text) {
            foreach (TextMeshProUGUI content in PageContents) {
                content.text = text;
                content.ForceMeshUpdate();
                if (!content.isTextOverflowing) break;

                text = text.Substring(content.firstOverflowCharacterIndex); // Doesn't update until frame passes
                content.text = content.text.Substring(0, content.firstOverflowCharacterIndex);
            }
            if (text.Length > 0) Debug.LogWarning($"Note couldn't fit entire text from scriptable object. Reduce total text or change entry preset to fit properly.");
        }

    }
}
