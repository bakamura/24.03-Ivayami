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
        private int _displayedSaveId;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        private void Awake() {
            PlayerActions.Instance.ChangeInputMap("Menu");
        }

        public void DisplaySaveInfo(int saveId) {
            _displayedSaveId = saveId;
            ChapterDescription chapterDescription = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{_displayedSaveId}");
            _previewImage.sprite = chapterDescription.Image;
            _previewText.text = chapterDescription.Text;

            Logger.Log(LogType.UI, $"Display Save {saveId}");
        }

        public void EnterSave() {
            SaveSystem.Instance.LoadSave((byte) _displayedSaveId);

            Logger.Log(LogType.UI, $"Display Save {_displayedSaveId}");
        }

    }
}