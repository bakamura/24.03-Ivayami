using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Ivayami.Scene;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField] private RectTransform _mapRectTranform;
        [SerializeField] private RectTransform _playerPointer;
        [SerializeField, Tooltip("Every pointer should be named Pointer_{ProgressNameCaseSensitive}")] private RectTransform[] _goalPointers;

        [SerializeField] private Vector2 _mapWorldSize;

        [Header("Open Map")]

        [SerializeField] private InputActionReference _openMapInput;
        [SerializeField] private Button _openMapBtn;

        [Header("Cache")]

        private Transform _cam;

        private void Awake() {
            _openMapInput.action.performed += OpenMap;

            _cam = Camera.main.transform;
        }

        public void PointersUpdate() {
            //Vector2 playerPosInMap = Vector2.zero;
            //playerPosInMap[0] = PlayerMovement.Instance.transform.position.x / _mapWorldSize.x;
            //playerPosInMap[1] = PlayerMovement.Instance.transform.position.z / _mapWorldSize.y;
            //playerPosInMap *= _mapRectTranform.sizeDelta;

            //_playerPointer.anchoredPosition = playerPosInMap;
            //_playerPointer.rotation = Quaternion.Euler(0f, 0f, _cam.transform.eulerAngles.y); //

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

    }
}