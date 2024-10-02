using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Ivayami.Scene;
using Ivayami.Save;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [Header("Pointers")]

        [SerializeField, Tooltip("Every pointer should be named Pointer_{ProgressNameCaseSensitive}")] private RectTransform[] _goalPointers;
        [SerializeField, Tooltip("Every blocker should be named Blocker_{tool}_{id}")] private GameObject[] _roadBlockers;

        [Header("Open Map")]

        [SerializeField] private RectTransform _mapRectTranform;
        [SerializeField] private ScrollRect _mapScrollRect;
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
            foreach (GameObject blocker in _roadBlockers) {
                blocker.SetActive(SaveSystem.Instance.Progress.id == 1); // id should be substituted for the blockers save
            }
        }

        private void OpenMap(InputAction.CallbackContext context) {
            if (!Pause.Instance.Paused) {
                Pause.Instance.PauseGame(true);
                _openMapBtn.onClick.Invoke();
            }
        }

        public void RecenterMap() {
            _mapScrollRect.StopMovement();
            _mapRectTranform.anchoredPosition = Vector2.zero;
        }

    }
}