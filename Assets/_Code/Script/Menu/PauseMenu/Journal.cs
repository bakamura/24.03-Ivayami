using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class Journal : MonoBehaviour {

        [SerializeField] private Image _entryImage;
        [SerializeField] private TextMeshProUGUI _entryNotes;
        [SerializeField] private Animator _containerAnimator;

        private static int _containerChange = Animator.StringToHash("Forward");

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";
        private const string CHARACTER_DESCRIPTION_FOLDER = "CharacterDescription";
        private const string DOCUMENT_FOLDER = "Document";
        private const string ABERRATION_DESCRIPTION_FOLDER = "AberrationDescription";

        public void ChangeAnimation() {
            _containerAnimator.SetTrigger(_containerChange);
        }

        public void FocusChapter(int chapterId) {
            ChapterDescription description = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{chapterId}");
            if(description == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;

            Logger.Log(LogType.UI, $"Journal - Focus Chapter {chapterId}");
        }

        public void FocusCharacter(int characterId) {
            ChapterDescription description = Resources.Load<ChapterDescription>($"{CHARACTER_DESCRIPTION_FOLDER}/CharacterDescription_{(true ? characterId : "null")}");
            if (description == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;

            Logger.Log(LogType.UI, $"Journal - Focus Character {characterId}");
        }

        public void FocusDocument(int documentId) {
            ChapterDescription description = Resources.Load<ChapterDescription>($"{DOCUMENT_FOLDER}/Document_{(true ? documentId : "null")}");
            if (description == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;

            Logger.Log(LogType.UI, $"Journal - Focus Document {documentId}");
        }

        public void FocusAberration(int aberrationId) {
            ChapterDescription description = Resources.Load<ChapterDescription>($"{ABERRATION_DESCRIPTION_FOLDER}/AberrationDescription_{(true ? aberrationId : "null")}");
            if (description == null) {
                Debug.LogWarning("Description Not Found");
                return;
            }
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;

            Logger.Log(LogType.UI, $"Journal - Focus Aberration {aberrationId}");
        }

    }
}