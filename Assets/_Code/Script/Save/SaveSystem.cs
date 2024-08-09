using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Ivayami.Save {
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

        private void Start() {
            SavePoint.onSaveGame.AddListener(SaveProgress);
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
                if (type == typeof(SaveProgress)) Progress = new SaveProgress(byte.Parse(savePath.Split('_')[1]));
                else Options = new SaveOptions();
                loadSaveCallback?.Invoke();

                Debug.Log($"No Save of type '{type.Name}' in {savePath}");
            }
        }

        public void LoadSavesProgress(Action<SaveProgress[]> loadSaveCallback) {
            StartCoroutine(LoadSavesProgressRoutine(loadSaveCallback));
        }

        private IEnumerator LoadSavesProgressRoutine(Action<SaveProgress[]> loadSaveCallback) {
            List<SaveProgress> progressSaves = new List<SaveProgress>();
            int saveId = 0;
            while (true) {
                if (File.Exists($"{_progressPath}/Save_{saveId}")) {
                    Task<string> readTask = File.ReadAllTextAsync($"{_progressPath}/Save_{saveId}");

                    yield return readTask;

                    progressSaves.Add(JsonUtility.FromJson<SaveProgress>(Encryption.Decrypt(readTask.Result)));
                    saveId++;
                }
                else break;
            }
            loadSaveCallback.Invoke(progressSaves.ToArray());

        }

        private void SaveProgress() {
            Progress.lastPlayedDate = DateTime.Now.ToString("dd/MM/yy [HH:mm]");
            StartCoroutine(WriteSaveRoutine($"{_progressPath}/Save_{Progress.id}", typeof(SaveProgress)));

            Logger.Log(LogType.Save, $"Writing Progress for save {Progress.id}");
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

        public void DeleteProgress(byte saveId) {
            string path = $"{_progressPath}/Save_{saveId}";
            if (File.Exists(path)) File.Delete(path);
        }

    }
}