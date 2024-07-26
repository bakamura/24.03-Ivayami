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

        protected override void Awake() {
            base.Awake();

            StartCoroutine(WaitForSaveOptions());

            PlayerActions.Instance.ChangeInputMap("Menu");
        }

        private void Start() {
            Options.OnChangeLanguage.AddListener((language) => SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate));
        }

        private IEnumerator WaitForSaveOptions() {
            while(SaveSystem.Instance.Options == null) yield return null;

            SaveSystem.Instance.LoadSavesProgress(SaveSelectBtnUpdate);
        }

        private void SaveSelectBtnUpdate(SaveProgress[] progressSaves) {
            for (int i = 0; i < _saveSelectBtns.Length; i++) _saveSelectBtns[i].Setup(i < progressSaves.Length ? progressSaves[i] : null);
        }

        public void DisplaySaveInfo(int saveId) {
            SaveSystem.Instance.LoadProgress((byte)saveId, DisplaySaveInfoCallback);

            Logger.Log(LogType.UI, $"Try Display Save {saveId}");
        }

        private void DisplaySaveInfoCallback() {
            ChapterDescription chapterDescription = Resources.Load<ChapterDescription>(string.IsNullOrEmpty(SaveSystem.Instance.Progress.lastProgressType) ?
                  $"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_New"
                : $"{CHAPTER_DESCRIPTION_FOLDER}/ChapterDescription_{SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}");
            _previewImage.sprite = chapterDescription.Image;
            _previewText.text = chapterDescription.Text;

            Logger.Log(LogType.UI, $"Displayed Save {SaveSystem.Instance.Progress.id} {(string.IsNullOrEmpty(SaveSystem.Instance.Progress.lastProgressType) ? "(New)": $"(Progress: {SaveSystem.Instance.Progress.lastProgressType}-{SaveSystem.Instance.Progress.progress[SaveSystem.Instance.Progress.lastProgressType]}")})");
        }

    }
}