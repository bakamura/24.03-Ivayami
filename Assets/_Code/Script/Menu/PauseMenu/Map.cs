using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Ivayami.Scene;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [Header("Pointers")]

        [SerializeField, Tooltip("Every pointer should be named Pointer_{ProgressNameCaseSensitive}")] private RectTransform[] _goalPointers;

        [Header("Open Map")]

        [SerializeField] private RectTransform _mapRectTranform;
        [SerializeField] private InputActionReference _openMapInput;
        [SerializeField] private Button _openMapBtn;

        [Header("Cache")]

        private Transform _cam;

        private void Awake() {
            _openMapInput.action.performed += OpenMap;

            _cam = Camera.main.transform;
        }

        public void PointersUpdate() {
            foreach (RectTransform goalPointer in _goalPointers) {
                Vector2 goalPosInMap = SceneController.Instance.PointerInChapter(goalPointer.name.Split('_')[1]);
                goalPointer.gameObject.SetActive(goalPosInMap != Vector2.zero);
                if (goalPosInMap != Vector2.zero) goalPointer.anchoredPosition = goalPosInMap;
            }
        }

        private void OpenMap(InputAction.CallbackContext context) {
            if (!Pause.Instance.Paused) {
                Pause.Instance.PauseGame(true);
                _openMapBtn.onClick.Invoke();
            }
        }

        public void RecenterMap() {
            _mapRectTranform.anchoredPosition = Vector2.zero;
        }

    }
}