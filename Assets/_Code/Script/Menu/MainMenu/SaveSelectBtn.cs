using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Save;

namespace Ivayami.UI {
    public class SaveSelectBtn : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private Image _chapterPreviewImage;
        [SerializeField] private TextMeshProUGUI _chapterNumberText;
        [SerializeField] private TextMeshProUGUI _saveDateText;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        public void Setup(SaveProgress progress) {
            _chapterNumberText.text = progress != null ? $"Save {progress.id}" : "New Game";
            _saveDateText.text = progress != null ? progress.lastPlayedDate : "";
            if (progress != null)_chapterPreviewImage.sprite = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}").Image;
            else _chapterPreviewImage.enabled = false;
        }

    }
}
