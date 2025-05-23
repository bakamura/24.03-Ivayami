using UnityEngine;
using UnityEngine.Localization.Components;
using TMPro;
using Ivayami.Save;
using Ivayami.Player;
using Ivayami.Player.Ability;

namespace Ivayami.UI {
    public class SaveSelectBtn : MonoBehaviour {

        [Header("UI")]

        [SerializeField] private LocalizeStringEvent _statusTextEvent;
        [SerializeField] private TextMeshProUGUI _dateText;
        private byte _id;
        private bool _isFirstTime;
        public Sprite PlaceImage { get; private set; }
        public string PlaceEntryName { get; private set; }
        public bool HasSaveInSlot { get; private set; }

        public void Setup(SaveProgress progress, byte id) {
            _id = id;
            _isFirstTime = progress == null;
            HasSaveInSlot = !_isFirstTime;
            _dateText.text = _isFirstTime ? "" : progress.lastPlayedDate;
            // Playtime (?)
            _statusTextEvent.SetEntry($"SaveSelectBtn/{(_isFirstTime ? "NewGame" : "Continue")}");
            PlaceEntryName = _isFirstTime ? $"SaveSelectBtn/NewGameMessage" : $"SaveSelectBtn/SavePoint_{progress.pointId}";
            PlaceImage = _isFirstTime ? null : Resources.Load<Sprite>($"PlacePreview/SavePoint_{progress.pointId}");
        }

        public void EnterSave() {
            SceneTransition.Instance.OnOpenEnd.AddListener(EnterSaveWaitFadeIn);
            SceneTransition.Instance.Open();
        }

        private void EnterSaveWaitFadeIn() {
            SaveSystem.Instance.LoadProgress(_id, () => {
                PlayerInventory.Instance.LoadInventory(SaveSystem.Instance.Progress.GetItemsData());
                AbilityGiverRef giverRef = gameObject.AddComponent<AbilityGiverRef>();
                if (SaveSystem.Instance?.Progress?.abilities?.Length > 0) foreach (string ability in SaveSystem.Instance.Progress.abilities) giverRef.GiveAbility(ability);
                SaveSelector.Instance.MainMenuUnloader.UnloadScene();
                
                if (_isFirstTime) {
                    SaveSelector.Instance.CutsceneLoader.LoadScene();
                }
                else {
                    TeleportPlayer();
                    //SceneController.Instance.OnAllSceneRequestEnd += TeleportPlayer;
                    //SaveSelector.Instance.BaseTerrainLoader.LoadScene();
                }
            });
        }

        private void TeleportPlayer() {
            SavePoint.Points[SaveSystem.Instance.Progress.pointId].SpawnPoint.Teleport();
            PlayerActions.Instance.ChangeInputMap("Player");
            //SceneController.Instance.OnAllSceneRequestEnd -= TeleportPlayer;
        }

        //private void TeleportPlayerNextLoad() {
        //    SceneController.Instance.OnAllSceneRequestEnd += TeleportPlayer;
        //    SceneController.Instance.OnAllSceneRequestEnd -= TeleportPlayerNextLoad;
        //}

    }
}
