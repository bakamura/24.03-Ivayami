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
        private bool _isFirstTime;

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        public void Setup(SaveProgress progress) {
            _isFirstTime = progress == null;
            LanguageTypes language = (LanguageTypes) SaveSystem.Instance.Options.language;
            UiText uiText = language == LanguageTypes.ENUS ? _uiText : Resources.Load<UiText>($"UiText/{(language)}/{_uiText.name}");
            _chapterNumberText.text = _isFirstTime ? uiText.GetText("NewGame") : $"{uiText.GetText("Save")} {progress.id}";
            _saveDateText.text = _isFirstTime ? "" : progress.lastPlayedDate;
            if (!_isFirstTime)_chapterPreviewImage.sprite = Resources.Load<ChapterDescription>($"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}").Image;
            else _chapterPreviewImage.enabled = false;
        }

        public void EnterSave() {
            if (_isFirstTime) SaveSelector.Instance.FirstTimeFade.FadeIn();
            else SaveSelector.Instance.NormalFade.FadeIn();
        }

    }
}
