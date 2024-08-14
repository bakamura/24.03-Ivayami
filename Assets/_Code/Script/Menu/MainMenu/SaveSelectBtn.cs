using UnityEngine;
using TMPro;
using Ivayami.Save;

namespace Ivayami.UI {
    public class SaveSelectBtn : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private UiText _uiText;
        private bool _isFirstTime;
        public Sprite PlaceImage { get; private set; }
        public string PlaceName { get; private set; }

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        public void Setup(SaveProgress progress) {
            _isFirstTime = progress == null;
            _statusText.text = _uiText.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language).GetText(_isFirstTime ? "NewGame" : "Continue");
            _dateText.text = _isFirstTime ? "" : progress.lastPlayedDate;
            // Show Playtime
            PlaceImage = null;
            PlaceName = "Ohio";
        }

        public void EnterSave() {
            if (_isFirstTime) SaveSelector.Instance.FirstTimeFade.FadeIn();
            else SaveSelector.Instance.NormalFade.FadeIn();
        }

    }
}
