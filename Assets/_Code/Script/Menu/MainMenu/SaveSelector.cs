using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.Player;
using Ivayami.Save;
using Ivayami.Scene;

namespace Ivayami.UI {
    public class SaveSelector : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private Image _previewImage;
        [SerializeField] private TextMeshProUGUI _previewText;
        [SerializeField] private SaveSelectBtn[] _saveSelectBtns;

        [Header("Input Stopping")]

        [SerializeField] private InputActionReference _pauseInput;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        private void Awake() {
            SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate);

            PlayerActions.Instance.ChangeInputMap("Menu");
            _pauseInput.action.Disable();

            SceneController.Instance.OnAllSceneRequestEnd = () => SceneController.Instance.OnAllSceneRequestEnd = EnablePlayerInput;
        }

        private void SaveSelectBtnUpdate(SaveProgress[] progressSaves) {
            for (int i = 0; i < _saveSelectBtns.Length; i++) _saveSelectBtns[i].Setup(i < progressSaves.Length ? progressSaves[i] : null);
        }

        public void DisplaySaveInfo(int saveId) {
            SaveSystem.Instance.LoadProgress((byte)saveId, DisplaySaveInfoCallback);

            Logger.Log(LogType.UI, $"Try Display Save {saveId}");
        }

        private void DisplaySaveInfoCallback() {
            ChapterDescription chapterDescription = Resources.Load<ChapterDescription>(string.IsNullOrEmpty(SaveSystem.Instance.Progress.lastProgressType) ?
                  $"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_New"
                : $"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}");
            _previewImage.sprite = chapterDescription.Image;
            _previewText.text = chapterDescription.Text;

            Logger.Log(LogType.UI, $"Displayed Save {SaveSystem.Instance.Progress.id} {(string.IsNullOrEmpty(SaveSystem.Instance.Progress.lastProgressType) ? "(New)": $"(Progress: {SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}")})");
        }

        public void EnablePlayerInput() {
            _pauseInput.action.Enable();
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerMovement.Instance.ToggleMovement(true);
            PlayerMovement.Instance.transform.position = SavePoint.Points[SaveSystem.Instance.Progress.pointId].spawnPoint.position;
            SceneController.Instance.OnAllSceneRequestEnd = null;
            SceneTransition.Instance.Transition();

            Logger.Log(LogType.UI, $"EnablePlayerInput callback");
        }

    }
}