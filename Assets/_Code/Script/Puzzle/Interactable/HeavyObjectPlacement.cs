using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using System.Collections;

namespace Ivayami.Puzzle {
    [RequireComponent(typeof(HeavyObjectSaver))]
    public class HeavyObjectPlacement : Activator, IInteractable {

        public static UnityEvent<bool> onCollect = new UnityEvent<bool>();

        [SerializeField] private string _correctName;
        [SerializeField] private Transform _placementPos;
        private GameObject _heavyObjectCurrent;
        public string HeavyObjectCurrentName { get { return _heavyObjectCurrent?.name; } }
        private Collider _collider;
        [SerializeField] private GameObject _interactPopup;

        [Space(24)]

        [SerializeField, Range(0f, 1f)] private float _pickupAnimationPoint;
        private WaitForSeconds _collectWait;
        private WaitForSeconds _placeWait;

        private InteractableFeedbacks _interactableFeedbacks;
        public InteractableFeedbacks InteratctableFeedbacks { get { return _interactableFeedbacks; } }

        private void Awake() {
            if (!TryGetComponent<InteractableFeedbacks>(out _interactableFeedbacks)) Debug.LogError($"'{name}' has no InteractableFeedbacks attached to!");
            if (!TryGetComponent<Collider>(out _collider)) Debug.LogError($"'{name}' has no Collider attached to!");
            onCollect.AddListener((isCollecting) => {
                bool shouldShow = _heavyObjectCurrent ^ isCollecting;
                _collider.enabled = shouldShow;
                _interactPopup.SetActive(shouldShow);
            });
        }

        private void Start() {
            float interactAnimDuration = PlayerAnimation.Instance ? PlayerAnimation.Instance.GetInteractAnimationDuration(PlayerActions.InteractAnimation.HeavyPickup) : 1f;
            _collectWait = new WaitForSeconds(interactAnimDuration * _pickupAnimationPoint);
            _placeWait = new WaitForSeconds(interactAnimDuration * (1f - _pickupAnimationPoint));
        }

        public void Setup() {
            StartCoroutine(SetupRoutine());
        }

        private IEnumerator SetupRoutine() {
            yield return null;

            if (_placementPos) {
                if (_placementPos.childCount > 0) _heavyObjectCurrent = _placementPos.GetChild(0).gameObject;
            }
            else Debug.LogError($"{name} has no _placementPos assigned!");
            bool enabled = _heavyObjectCurrent != null;
            _collider.enabled = enabled;
            _interactPopup.SetActive(enabled);
        }

        public PlayerActions.InteractAnimation Interact() {
            if (PlayerActions.Instance.IsHoldingHeavyObject) {
                if (TryPlace()) return PlayerActions.InteractAnimation.HeavyPlace;
            }
            else if (TryCollect()) return PlayerActions.InteractAnimation.HeavyPickup;
            return PlayerActions.InteractAnimation.Default;
        }

        public bool TryCollect() {
            if (_heavyObjectCurrent) {
                StartCoroutine(CollectRoutine());
                return true;
            }
            else Debug.LogError($"Could not Collect from '{name}': Place empty.");
            return false;
        }

        private IEnumerator CollectRoutine() {
            yield return _collectWait;

            PlayerActions.Instance.HeavyObjectHold(_heavyObjectCurrent);
            _heavyObjectCurrent = null;
            IsActive = false;

            PlayerMovement.Instance.AllowRun(false);
            PlayerMovement.Instance.AllowCrouch(false);
            PlayerStress.Instance.onFail.AddListener(RemovePlayerObject);
            PlayerAnimation.Instance.HeavyHold(true);
            onCollect.Invoke(true);
            _interactableFeedbacks.UpdateFeedbacks(true, false, true);
        }

        public bool TryPlace() {
            if (!_heavyObjectCurrent) {
                StartCoroutine(PlaceRoutine());
                return true;
            }
            else Debug.LogError($"Could not Place to '{name}': Place not empty.");
            return false;
        }

        private IEnumerator PlaceRoutine() {
            yield return _placeWait;

            _heavyObjectCurrent = PlayerActions.Instance.HeavyObjectRelease();
            _heavyObjectCurrent.transform.parent = _placementPos;
            _heavyObjectCurrent.transform.localPosition = Vector3.zero;
            _heavyObjectCurrent.transform.rotation = Quaternion.identity;
            CheckForActivation();

            PlayerMovement.Instance.AllowRun(true);
            PlayerMovement.Instance.AllowCrouch(true);
            PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
            PlayerAnimation.Instance.HeavyHold(false);
            onCollect.Invoke(false);
            _interactableFeedbacks.UpdateFeedbacks(true, false, true);
        }

        private void CheckForActivation() {
            if (_heavyObjectCurrent?.name == _correctName) {
                IsActive = true;
                onActivate.Invoke();
            }
        }

        private void RemovePlayerObject() {
            StopAllCoroutines();
            PlayerActions.Instance.HeavyObjectRelease().transform.parent = _placementPos;
            PlayerMovement.Instance.AllowRun(true);
            PlayerMovement.Instance.AllowCrouch(true);
            PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
            PlayerAnimation.Instance.HeavyHold(false);
        }

    }
}
