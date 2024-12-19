using UnityEngine;
using TMPro;
using Ivayami.Save;
using Ivayami.Scene;
using Ivayami.Player;
using UnityEngine.Localization;

namespace Ivayami.UI {
    public class SaveSelectBtn : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private LocalizedString _newGameString;
        [SerializeField] private LocalizedString _continueGameString;
        private byte _id;
        private bool _isFirstTime;
        public Sprite PlaceImage { get; private set; }
        public string PlaceName { get; private set; }

        public void Setup(SaveProgress progress, byte id) {
            _id = id;
            _isFirstTime = progress == null;
            _statusText.text = _isFirstTime ? _newGameString.GetLocalizedString() : _continueGameString.GetLocalizedString();
            _dateText.text = _isFirstTime ? "" : progress.lastPlayedDate;
            // Show Playtime
            PlaceName = _isFirstTime ? "NewGameMessage" : progress.lastSavePlace;
            PlaceImage = _isFirstTime ? null : Resources.Load<Sprite>($"PlacePreview/{progress.lastSavePlace}");
        }

        public void EnterSave() {
            SceneTransition.Instance.OnOpenEnd.AddListener(EnterSaveWaitFadeIn);
            SceneTransition.Instance.Open();
        }

        private void EnterSaveWaitFadeIn() {
            SaveSystem.Instance.LoadProgress(_id, () => {
                PlayerInventory.Instance.LoadInventory(SaveSystem.Instance.Progress.GetItemsData());

                SaveSelector.Instance.MainMenuUnloader.UnloadScene();
                if (_isFirstTime) {
                    SceneController.Instance.OnAllSceneRequestEnd += TeleportPlayerNextLoad;
                    SaveSelector.Instance.CutsceneLoader.LoadScene();
                }
                else {
                    SceneController.Instance.OnAllSceneRequestEnd += TeleportPlayer;
                    SaveSelector.Instance.BaseTerrainLoader.LoadScene();
                }
            });
        }

        private void TeleportPlayer() {
            SavePoint.Points[SaveSystem.Instance.Progress.pointId].SpawnPoint.Teleport();
            PlayerActions.Instance.ChangeInputMap("Player");
            SceneController.Instance.OnAllSceneRequestEnd -= TeleportPlayer;
        }

        private void TeleportPlayerNextLoad() {
            SceneController.Instance.OnAllSceneRequestEnd += TeleportPlayer;
            SceneController.Instance.OnAllSceneRequestEnd -= TeleportPlayerNextLoad;
        }

    }
}
