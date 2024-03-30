using UnityEngine;

namespace Paranapiacaba.Save {
    public class SaveSystem : MonoSingleton<SaveSystem> {

        public SaveProgress Progress { get; private set; }
        public SaveOptions Options { get; private set; }

        public void LoadSave(byte saveId) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void CompleteChapter(byte chapterId) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void SaveOptions() {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}