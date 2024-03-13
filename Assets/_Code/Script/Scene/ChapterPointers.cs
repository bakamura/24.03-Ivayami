using UnityEngine;

namespace Paranapiacaba.Scene {
    [CreateAssetMenu(menuName = "SceneControl/ChapterPointers")]
    public class ChapterPointers : ScriptableObject {

        public Vector3 playerPositionOnChapterStart;
        [SerializeField] private Vector2[] _mapPointerAtSubChapter;

        public void SubChapterPointer(byte subChapterId) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}