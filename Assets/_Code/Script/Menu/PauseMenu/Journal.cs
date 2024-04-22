using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Paranapiacaba.UI {
    public class Journal : MonoBehaviour {

        [SerializeField] private Image _entryImage;
        [SerializeField] private TextMeshProUGUI _entryNotes;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        public void ChangeAnimation() {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void FocusChapter(int chapterId) {
            ChapterDescription description = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{chapterId}-{0 /* Implement somehow */}");
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;
        }

        public void FocusCharacter(int chapterId) {
            // Change to other ScriptableObject
            ChapterDescription description = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{chapterId}-{0 /* Implement somehow */}");
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void FocusDocument(int chapterId) {
            // Change to other ScriptableObject
            ChapterDescription description = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{chapterId}-{0 /* Implement somehow */}");
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void FocusAberration(int chapterId) {
            // Change to other ScriptableObject
            ChapterDescription description = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{chapterId}-{0 /* Implement somehow */}");
            _entryImage.sprite = description.Image;
            _entryNotes.text = description.Text;
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}