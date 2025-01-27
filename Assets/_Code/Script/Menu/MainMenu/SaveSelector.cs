using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;
using Ivayami.Save;
using Ivayami.Scene;

namespace Ivayami.UI {
    public class SaveSelector : MonoSingleton<SaveSelector> {

        [Header("UI")]

        [SerializeField] private Image _previewImage;
        [SerializeField] private TextMeshProUGUI _previewText;
        [SerializeField] private SaveSelectBtn[] _saveSelectBtns;

        //Game Entering

        [field: SerializeField] public SceneLoader BaseTerrainLoader { get; private set; }
        [field: SerializeField] public SceneLoader CutsceneLoader { get; private set; }
        [field: SerializeField] public SceneLoader MainMenuUnloader { get; private set; }

        private const string BLOCKER_KEY = "MainMenu";

        private void Start() {
            StartCoroutine(WaitForSaveOptions());

            Options.OnChangeLanguage.AddListener(() => SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate));
            PlayerActions.Instance.ChangeInputMap("Menu");
            PlayerMovement.Instance.ToggleMovement(BLOCKER_KEY, false);
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, false);
        }

        private IEnumerator WaitForSaveOptions() {
            while(SaveSystem.Instance.Options == null) yield return null;

            SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate);
        }

        private void SaveSelectBtnUpdate(SaveProgress[] progressSaves) {
            bool foundSave;
            for (int i = 0; i < _saveSelectBtns.Length; i++)
            {
                foundSave = false;
                for(int a = 0; a < progressSaves.Length; a++)
                {
                    if(progressSaves[a].id == i)
                    {
                        foundSave = true;
                        _saveSelectBtns[i].Setup(progressSaves[a], progressSaves[a].id);
                        break;
                    }
                }
                if (!foundSave) _saveSelectBtns[i].Setup(null, (byte)i);
            }
        }

        public void DisplaySaveInfo(int saveId) {
            _previewImage.sprite = _saveSelectBtns[saveId].PlaceImage;
            _previewImage.color = _previewImage.sprite != null ? Color.white : new Color(0, 0, 0, 0);
            _previewText.text = _saveSelectBtns[saveId].PlaceName;

            Logger.Log(LogType.UI, $"Display Save {saveId}");
        }

        public void RemovePlayerBlocker() {
            PlayerMovement.Instance.ToggleMovement(BLOCKER_KEY, true);
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_saveSelectBtns.Length > SaveSystem.MaxSaveSlots) System.Array.Resize(ref _saveSelectBtns, SaveSystem.MaxSaveSlots);
        }
#endif
    }
}