using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace Paranapiacaba.Save {
    public class SaveSystem : MonoSingleton<SaveSystem> {

        public SaveProgress Progress { get; private set; }
        public SaveOptions Options { get; private set; }

        private string _optionsPath;
        private const string SAVE_FOLDER = "Saves";

        public SaveSystem() {
            _optionsPath = $"{SAVE_FOLDER}/Configs";
        }

        public void LoadSave(byte saveId) {
            StartCoroutine(LoadSaveRoutine(saveId));
        }

        private IEnumerator LoadSaveRoutine(byte saveId) {
            string savePath = $"{SAVE_FOLDER}/Save_{saveId}";
            if (File.Exists(savePath)) {
                Task<string> readTask = File.ReadAllTextAsync(savePath);

                yield return readTask;

                Progress = JsonUtility.FromJson<SaveProgress>(readTask.Result);
            }
            else Debug.LogError($"No save with ID: '{saveId}'");
        }

        private void WriteSave(byte saveId) {
            string savePath = $"{SAVE_FOLDER}/Save_{saveId}";
            File.WriteAllText(savePath, JsonUtility.ToJson(Progress));
        }

        private void LoadOptions() {
            if (File.Exists(_optionsPath)) Options = JsonUtility.FromJson<SaveOptions>(_optionsPath);
            else Options = new();
        }

        public void CompleteChapter(byte chapterId) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void SaveOptions() {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}