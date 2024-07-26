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
        [SerializeField] private UiText _uiText;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        public void Setup(SaveProgress progress) {
            LanguageTypes language = (LanguageTypes) SaveSystem.Instance.Options.language;
            UiText uiText = language == LanguageTypes.ENUS ? _uiText : Resources.Load<UiText>($"UiText/{(language)}/{_uiText.name}");
            _chapterNumberText.text = progress != null ? $"{uiText.GetText("Save")} {progress.id}" : uiText.GetText("NewGame");
            _saveDateText.text = progress != null ? progress.lastPlayedDate : "";
            if (progress != null)_chapterPreviewImage.sprite = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}").Image;
            else _chapterPreviewImage.enabled = false;
        }

    }
}
