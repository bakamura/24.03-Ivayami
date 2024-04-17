using Paranapiacaba.Player;
using Paranapiacaba.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Paranapiacaba.UI {
    public class SaveSelector : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private Image _previewImage;
        [SerializeField] private TextMeshProUGUI _previewText;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        private void Awake() {
            PlayerActions.Instance.ChangeInputMap("Menu");
        }

        public void DisplaySaveInfo(int saveId) {
            SaveSystem.Instance.LoadProgress((byte)saveId, DisplaySaveInfoCallback);

            Logger.Log(LogType.UI, $"Try Display Save {saveId}");
        }

        private void DisplaySaveInfoCallback() {
            ChapterDescription chapterDescription = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.currentChapter}");
            _previewImage.sprite = chapterDescription.Image;
            _previewText.text = chapterDescription.Text;

            Logger.Log(LogType.UI, $"Displayed Save {SaveSystem.Instance.Progress.currentChapter}");
        }

        public void EnterSave() {


            Logger.Log(LogType.UI, $"Entering Save");
        }

    }
}