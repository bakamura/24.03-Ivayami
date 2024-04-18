using Paranapiacaba.Scene;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class Map : MonoBehaviour {

        [SerializeField] private RectTransform _playerPointer;
        [SerializeField] private RectTransform _goalPointer;

        public void PointersUpdate() {
            _playerPointer.anchoredPosition = Vector2.zero;
            _playerPointer.rotation = Quaternion.Euler(0f, 0f, 1f);

            _goalPointer.anchoredPosition = SceneController.Instance.PointerInChapter(0, 0);
        }

    }
}