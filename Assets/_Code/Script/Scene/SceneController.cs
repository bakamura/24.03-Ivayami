using UnityEngine;

namespace Paranapiacaba.Scene {
    public class SceneController : MonoSingleton<SceneController> {

        [SerializeField] private ChapterPointers[] _chapterPointers;

        public void LoadBaseScene() {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void StartLoad(byte sceneId) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void PointerInChapter(byte chapter, byte subChapter) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}