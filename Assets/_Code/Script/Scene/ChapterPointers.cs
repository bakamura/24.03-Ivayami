using UnityEngine;

namespace Ivayami.Scene {
    [CreateAssetMenu(menuName = "SceneControl/ChapterPointers")]
    public class ChapterPointers : ScriptableObject {

        public Vector3 playerPositionOnChapterStart;
        [SerializeField] private Vector2[] _mapPointerAtSubChapter;

        public Vector2 SubChapterPointer(byte subChapterId) {
            return _mapPointerAtSubChapter[subChapterId];
        }

    }
}