using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.UI {
    public class SaveSelector : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private Image _previewImage;
        [SerializeField] private TextMeshProUGUI _previewText;

        [Header("Input Stopping")]

        [SerializeField] private InputActionReference _pauseInput;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        private void Awake() {
            PlayerActions.Instance.ChangeInputMap("Menu");
            _pauseInput.action.Disable();
        }

        public void DisplaySaveInfo(int saveId) {
                SaveSystem.Instance.LoadProgress((byte)saveId, DisplaySaveInfoCallback);

                Logger.Log(LogType.UI, $"Try Display Save {saveId}");
        }

        private void DisplaySaveInfoCallback() {
            ChapterDescription chapterDescription = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.currentChapter}-{SaveSystem.Instance.Progress.currentSubChapter}");
            _previewImage.sprite = chapterDescription.Image;
            _previewText.text = chapterDescription.Text;

            Logger.Log(LogType.UI, $"Displayed Save {SaveSystem.Instance.Progress.id} (Progress: {SaveSystem.Instance.Progress.currentChapter}-{SaveSystem.Instance.Progress.currentSubChapter})");
        }

        public void EnterSave() {
            _pauseInput.action.Enable();
            PlayerActions.Instance.ChangeInputMap("Player"); //

            Logger.Log(LogType.UI, $"Entering Save");
        }

    }
}