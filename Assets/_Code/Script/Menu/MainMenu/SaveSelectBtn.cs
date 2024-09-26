using UnityEngine;
using TMPro;
using Ivayami.Save;
using Ivayami.Scene;
using Ivayami.Dialogue;
using Ivayami.Player;

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
            PlaceImage = _isFirstTime ? null : Resources.Load<Sprite>($"PlacePreview/{progress.lastSavePlace}");
        }

        public void EnterSave() {
            SceneTransition.Instance.OnCloseEnd.AddListener(EnterSaveWaitFadeIn);
            SceneTransition.Instance.Close();
        }

        private void EnterSaveWaitFadeIn() {
            SaveSystem.Instance.LoadProgress(_id, () => {
                PlayerInventory.Instance.LoadInventory(SaveSystem.Instance.Progress.inventory);

                SaveSelector.Instance.MainMenuUnloader.UnloadScene();
                if (_isFirstTime) SaveSelector.Instance.CutsceneLoader.LoadScene();
                else {
                    SceneController.Instance.OnAllSceneRequestEnd += TeleportPlayer;
                    SaveSelector.Instance.BaseTerrainLoader.LoadScene();
                }
            });
        }

        private void TeleportPlayer() {
            if (!CutsceneController.IsPlaying) {
                SavePoint.Points[SaveSystem.Instance.Progress.pointId].SpawnPoint.Teleport();
                SceneController.Instance.OnAllSceneRequestEnd -= TeleportPlayer;
            }
        }

    }
}
