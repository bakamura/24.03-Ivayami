using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Ivayami.Scene;
using Ivayami.Save;
using System.Collections;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [Header("Pointers")]

        [SerializeField, Tooltip("Every pointer should be named Pointer_{ProgressNameCaseSensitive}")] private RectTransform[] _goalPointers;
        [SerializeField, Tooltip("Every blocker should be named {uniqueNumber}_AnyName")] private GameObject[] _roadBlockers;

        [Header("Map Components")]

        [SerializeField] private RectTransform _mapRectTranform;
        [SerializeField] private ScrollRect _mapScrollRect;

        [Header("Map Controls")]

        [SerializeField] private InputActionReference _moveMapInput;
        [SerializeField, Min(1f)] private float _controlDragSensitivity = 5;
        [SerializeField] private InputActionReference _zoomMapInput;
        [SerializeField, Min(0f)] private float _controlerZoomMapSensitivity;
        [SerializeField] private float _scrollZoomMapSensitivity;
        [SerializeField, Min(1f)] private float _maxMapZoom;

        [Header("Quick Open")]

        [SerializeField] private InputActionReference _quickOpenInput;
        [SerializeField] private Button _quickOpenBtn;

        [Header("Cache")]

        private Vector2 _currentMovemapInput;
        private Vector2 _currentZoomMapInput;
        private Vector2 _initialMapSize;
        private RectTransform _mapParentTransform;
        private float _currentZoomMapSensitivity = 1f;

        private void Awake() {
            _mapParentTransform = _mapRectTranform.parent.GetComponent<RectTransform>();
            _initialMapSize = _mapParentTransform.rect.size;
        }

        private void Start() {
            InputCallbacks.Instance.SubscribeToOnChangeControls(HandleControlChange);
            _quickOpenInput.action.performed += QuickOpen;
        }

        public void PointersUpdate() {
            foreach (RectTransform goalPointer in _goalPointers) {
                Vector2 goalPosInMap = SceneController.Instance.PointerInChapter(goalPointer.name.Split('_')[1]);
                goalPointer.gameObject.SetActive(goalPosInMap != Vector2.zero);
                if (goalPosInMap != Vector2.zero) goalPointer.anchoredPosition = goalPosInMap;
            }
            foreach (GameObject blocker in _roadBlockers) {
                if (int.TryParse(blocker.name.Split('_')[0], out int id)) blocker.SetActive(SaveSystem.Instance.Progress.GetRoadBlockerState(id) == RoadBlocker.State.Discovered);
                else Debug.LogWarning($"Couldn't get ID of road blocker indicator '{blocker.name}', make sure the object is named like '7_AnyNameReally'");
            }
        }

        public void UpdateInputs(bool isActive) {
            if (isActive) {
                _moveMapInput.action.performed += MovementMap;
                _zoomMapInput.action.performed += ZoomMap;
                StartCoroutine(MoveMapCoroutine());
                StartCoroutine(ZoomMapCoroutine());
            }
            else {
                _moveMapInput.action.performed -= MovementMap;
                _zoomMapInput.action.performed -= ZoomMap;
                StopCoroutine(MoveMapCoroutine());
                StopCoroutine(ZoomMapCoroutine());
                RecenterMap();
            }
        }

        private void ZoomMap(InputAction.CallbackContext obj) {
            _currentZoomMapInput = obj.ReadValue<Vector2>();
        }

        private void MovementMap(InputAction.CallbackContext obj) {
            _currentMovemapInput = obj.ReadValue<Vector2>();
        }

        private IEnumerator MoveMapCoroutine() {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            while (true) {
                //check value if is greater than controller deadzone
                if (_currentMovemapInput != Vector2.zero) _mapRectTranform.anchoredPosition += _controlDragSensitivity * Time.fixedDeltaTime * _currentMovemapInput;
                yield return delay;
            }
        }

        private IEnumerator ZoomMapCoroutine() {
            float currentStep = 0;
            float currentScale;
            while (true) {
                if (_currentZoomMapInput != Vector2.zero) {
                    currentStep = Mathf.Clamp(currentStep + Time.deltaTime * _currentZoomMapInput.y * _currentZoomMapSensitivity, 0f, 1f);
                    currentScale = Mathf.Lerp(1, _maxMapZoom, currentStep);
                    _mapParentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_mapParentTransform.rect.width, _currentZoomMapInput.y > 0 ? _initialMapSize.x / _maxMapZoom : _initialMapSize.x, currentStep));
                    _mapParentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(_mapParentTransform.rect.height, _currentZoomMapInput.y > 0 ? _initialMapSize.y / _maxMapZoom : _initialMapSize.y, currentStep));
                    _mapParentTransform.localScale = new Vector3(currentScale, currentScale, 1);
                }
                yield return null;
            }
        }

        private void ResetMapZoom() {
            _mapParentTransform.localScale = Vector3.one;
            _mapParentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialMapSize.x);
            _mapParentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _initialMapSize.y);
            _currentZoomMapInput = Vector2.zero;
        }

        private void QuickOpen(InputAction.CallbackContext context) {
            if (!Pause.Instance.Paused) {
                Pause.Instance.PauseGame(true);
                _quickOpenBtn.onClick.Invoke();
            }
        }

        public void RecenterMap() {
            ResetMapZoom();
            _currentMovemapInput = Vector2.zero;
            _mapScrollRect.StopMovement();
            _mapRectTranform.anchoredPosition = Vector2.zero;
        }

        private void HandleControlChange(InputCallbacks.ControlType controlType) {
            _currentZoomMapSensitivity = controlType == InputCallbacks.ControlType.Keyboard ? _scrollZoomMapSensitivity : _controlerZoomMapSensitivity;
        }

    }
}