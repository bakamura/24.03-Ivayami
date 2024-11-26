using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Ivayami.Scene;
using Ivayami.Save;
using System.Collections;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [Header("Pointers")]

        [SerializeField, Tooltip("Every pointer should be named Pointer_{ProgressNameCaseSensitive}")] private RectTransform[] _goalPointers;
        [SerializeField, Tooltip("Every blocker should be named Blocker_{tool}_{id}")] private GameObject[] _roadBlockers;

        [Header("Open Map")]

        [SerializeField] private RectTransform _mapRectTranform;
        [SerializeField] private ScrollRect _mapScrollRect;
        [SerializeField] private InputActionReference _openMapInput;
        [SerializeField] private InputActionReference _moveMapInput;
        [SerializeField, Min(1f)] private float _controlDragSensitivity = 5;
        [SerializeField] private Button _openMapBtn;

        [Header("Cache")]
        private Vector2 _currentInputValue;
        //private Transform _cam;

        private void Awake() {
            _openMapInput.action.performed += OpenMap;

            //_cam = Camera.main.transform;
        }

        public void PointersUpdate() {
            foreach (RectTransform goalPointer in _goalPointers) {
                Vector2 goalPosInMap = SceneController.Instance.PointerInChapter(goalPointer.name.Split('_')[1]);
                goalPointer.gameObject.SetActive(goalPosInMap != Vector2.zero);
                if (goalPosInMap != Vector2.zero) goalPointer.anchoredPosition = goalPosInMap;
            }
            foreach (GameObject blocker in _roadBlockers) {
                if (int.TryParse(blocker.name.Split('.')[0], out int id)) blocker.SetActive(SaveSystem.Instance.Progress.GetRoadBlockerState(id) == RoadBlocker.State.Discovered);
                else Debug.LogWarning($"Couldn't get ID of road blocker indicator '{blocker.name}', make sure the object is named like '7.AnyNameReally'");
            }
        }

        public void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _moveMapInput.action.performed += MovementMap;
                StartCoroutine(MoveMapCoroutine());
            }
            else
            {
                _moveMapInput.action.performed -= MovementMap;
                StopCoroutine(MoveMapCoroutine());
            }
        }

        private void MovementMap(InputAction.CallbackContext obj)
        {
            _currentInputValue = obj.ReadValue<Vector2>();
        }

        private IEnumerator MoveMapCoroutine()
        {            
            while (true)
            {
                //check value if is greater than controller deadzone
                if(_currentInputValue != Vector2.zero)_mapRectTranform.anchoredPosition += _currentInputValue * _controlDragSensitivity;
                yield return null;
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