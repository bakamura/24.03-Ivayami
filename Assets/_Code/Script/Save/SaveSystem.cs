using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Paranapiacaba.Save {
    public class SaveSystem : MonoSingleton<SaveSystem> {

        public SaveProgress Progress { get; private set; }
        public SaveOptions Options { get; private set; }

        private string _progressPath = $"{Application.persistentDataPath}/Progress";
        private string _optionsPath = $"{Application.persistentDataPath}/Configs";

        protected override void Awake() {
            if (!Directory.Exists(_progressPath)) Directory.CreateDirectory(_progressPath);
            LoadOptions();
        }

        public void LoadProgress(byte saveId, Action loadSaveCallback) {
            StartCoroutine(LoadSaveRoutine($"{_progressPath}/Save_{saveId}", typeof(SaveProgress), loadSaveCallback));
        }

        private void LoadOptions() {
            StartCoroutine(LoadSaveRoutine(_optionsPath, typeof(SaveOptions)));
        }

        private IEnumerator LoadSaveRoutine(string savePath, Type type, Action loadSaveCallback = null) {
            if (File.Exists(savePath)) {
                Task<string> readTask = File.ReadAllTextAsync(savePath);

                yield return readTask;

                if (type == typeof(SaveProgress)) Progress = JsonUtility.FromJson<SaveProgress>(readTask.Result);
                else Options = JsonUtility.FromJson<SaveOptions>(Encryption.Decrypt(readTask.Result));
                loadSaveCallback?.Invoke();
            }
            else Debug.Log($"No save of type '{type.Name}' in {savePath}");
        }

        private void SaveProgress(byte saveId) {
            StartCoroutine(WriteSaveRoutine($"{_progressPath}/Save_{saveId}", typeof(SaveProgress)));
        }

        public void SaveOptions() {
            StartCoroutine(WriteSaveRoutine(_optionsPath, typeof(SaveOptions)));
        }

        private IEnumerator WriteSaveRoutine(string savePath, Type type) {
             if(type == typeof(SaveProgress)) yield return File.WriteAllTextAsync(savePath, JsonUtility.ToJson(Progress));
             else yield return File.WriteAllTextAsync(savePath, Encryption.Encrypt(JsonUtility.ToJson(Options)));
        }

        public void CompleteChapter() {
            Progress.currentChapter++;
        }

        public void CompleteSubChapter() {
            Progress.currentSubChapter++;
        }

    }
}