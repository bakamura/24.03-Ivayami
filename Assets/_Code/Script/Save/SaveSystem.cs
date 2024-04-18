using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Paranapiacaba.Save {
    public class SaveSystem : MonoSingleton<SaveSystem> {

        public SaveProgress Progress { get; private set; }
        public SaveOptions Options { get; private set; }

        private string _progressPath;
        private string _optionsPath;

        protected override void Awake() {
            base.Awake();

            _progressPath = $"{Application.persistentDataPath}/Progress";
            _optionsPath = $"{Application.persistentDataPath}/Configs";

            if (!Directory.Exists(_progressPath)) Directory.CreateDirectory(_progressPath);
            LoadOptions();
        }

        public void LoadProgress(byte saveId, Action loadSaveCallback) {
            StartCoroutine(LoadSaveRoutine($"{_progressPath}/Save_{saveId}", typeof(SaveProgress), loadSaveCallback));

            Logger.Log(LogType.Save, $"Loading Progress for save {saveId}");
        }

        private void LoadOptions() {
            StartCoroutine(LoadSaveRoutine(_optionsPath, typeof(SaveOptions)));

            Logger.Log(LogType.Save, $"Loading Options Save");
        }

        private IEnumerator LoadSaveRoutine(string savePath, Type type, Action loadSaveCallback = null) {
            if (File.Exists(savePath)) {
                Task<string> readTask = File.ReadAllTextAsync(savePath);

                yield return readTask;

                if (type == typeof(SaveProgress)) Progress = JsonUtility.FromJson<SaveProgress>(Encryption.Decrypt(readTask.Result));
                else Options = JsonUtility.FromJson<SaveOptions>(Encryption.Decrypt(readTask.Result));
                loadSaveCallback?.Invoke();

                Logger.Log(LogType.Save, $"Loaded Save of type '{type.Name}' in {savePath}");
            }
            else {
                if (type == typeof(SaveProgress)) Progress = new SaveProgress();
                else Options = new SaveOptions();
                loadSaveCallback?.Invoke();

                Debug.Log($"No Save of type '{type.Name}' in {savePath}");
            }
        }

        private void SaveProgress(byte saveId) {
            StartCoroutine(WriteSaveRoutine($"{_progressPath}/Save_{saveId}", typeof(SaveProgress)));

            Logger.Log(LogType.Save, $"Writing Progress for save {saveId}");
        }

        public void SaveOptions() {
            StartCoroutine(WriteSaveRoutine(_optionsPath, typeof(SaveOptions)));

            Logger.Log(LogType.Save, $"Writing Options Save");
        }

        private IEnumerator WriteSaveRoutine(string savePath, Type type) {
            if (type == typeof(SaveProgress)) yield return File.WriteAllTextAsync(savePath, Encryption.Encrypt(JsonUtility.ToJson(Progress)));
            else yield return File.WriteAllTextAsync(savePath, Encryption.Encrypt(JsonUtility.ToJson(Options)));

            Logger.Log(LogType.Save, $"Wrote Save of type '{type.Name}' in {savePath}");
        }

        public void CompleteChapter() {
            Progress.currentChapter++;
            SaveProgress(Progress.id);
        }

        public void CompleteSubChapter() {
            Progress.currentSubChapter++;
            SaveProgress(Progress.id);
        }

    }
}