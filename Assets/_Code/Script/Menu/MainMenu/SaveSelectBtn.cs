using UnityEngine;
using TMPro;
using Ivayami.Save;

namespace Ivayami.UI {
    public class SaveSelectBtn : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private UiText _uiText;
        private byte _id;
        private bool _isFirstTime;
        public Sprite PlaceImage { get; private set; }
        public string PlaceName { get; private set; }

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        public void Setup(SaveProgress progress, byte id) {
            _id = id;
            UiText uiText = _uiText.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language);
            _isFirstTime = progress == null;
            _statusText.text = uiText.GetText(_isFirstTime ? "NewGame" : "Continue");
            _dateText.text = _isFirstTime ? "" : progress.lastPlayedDate;
            // Show Playtime
            PlaceName = uiText.GetText(_isFirstTime ? "NewGameMessage" : progress.lastSavePlace);
            PlaceImage = Resources.Load<Sprite>($"PlacePreview/{progress.lastSavePlace}");
        }

        public void EnterSave() {
            // Probably should fade in before start loading, then decide what to do
            SaveSystem.Instance.LoadProgress(_id, () => {
                if (_isFirstTime) SaveSelector.Instance.FirstTimeFade.FadeIn();
                else SaveSelector.Instance.NormalFade.FadeIn();
            });
        }

    }
}
