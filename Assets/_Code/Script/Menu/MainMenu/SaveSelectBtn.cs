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

        public void Setup(SaveProgress progress) {
            _chapterNumberText.text = progress != null ? $"Chapter {progress.currentChapter}" : "New Game";
            _saveDateText.text = progress != null ? progress.lastPlayedDate : "";
            if (progress != null)_chapterPreviewImage.sprite = Resources.Load<Sprite>($"ChapterPreviewImage/Chapter_{progress.currentChapter}");
            else _chapterPreviewImage.enabled = false;
        }

    }
}
