using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.UI {
    public class SaveSelector : MonoSingleton<SaveSelector> {

        [Header("UI")]

        [SerializeField] private Image _previewImage;
        [SerializeField] private TextMeshProUGUI _previewText;
        [SerializeField] private SaveSelectBtn[] _saveSelectBtns;

        //Game Entering

        [field: SerializeField] public ScreenFade FirstTimeFade { get; private set; }
        [field: SerializeField] public ScreenFade NormalFade { get; private set; }

        private const string CHAPTER_DESCRIPTION_FOLDER = "ChapterDescription";

        private void Start() {
            StartCoroutine(WaitForSaveOptions());

            Options.OnChangeLanguage.AddListener((language) => SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate));
            PlayerActions.Instance.ChangeInputMap("Menu");
        }

        private IEnumerator WaitForSaveOptions() {
            while(SaveSystem.Instance.Options == null) yield return null;

            SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate);
        }

        private void SaveSelectBtnUpdate(SaveProgress[] progressSaves) {
            for (int i = 0; i < _saveSelectBtns.Length; i++) _saveSelectBtns[i].Setup(i < progressSaves.Length ? progressSaves[i] : null, (byte)i);
        }

        public void DisplaySaveInfo(int saveId) {
            _previewImage.sprite = _saveSelectBtns[saveId].PlaceImage;
            _previewImage.color = _previewImage.sprite != null ? Color.white : new Color(0, 0, 0, 0);
            _previewText.text = _saveSelectBtns[saveId].PlaceName;

            Logger.Log(LogType.UI, $"Display Save {saveId}");
        }
    }
}