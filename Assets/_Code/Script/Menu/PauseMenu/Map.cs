using UnityEngine;
using Ivayami.Scene;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [SerializeField] private RectTransform _playerPointer;
        [SerializeField] private RectTransform _goalPointer;

        private Transform _cam;

        private void Awake() {
            _cam = Camera.main.transform;
        }

        public void PointersUpdate() {
            _playerPointer.anchoredPosition = Vector2.zero;
            _playerPointer.rotation = Quaternion.Euler(0f, 0f, _cam.transform.eulerAngles.y); //

            _goalPointer.anchoredPosition = SceneController.Instance.PointerInChapter();
        }

    }
}