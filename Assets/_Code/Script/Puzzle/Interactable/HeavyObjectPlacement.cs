using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class HeavyObjectPlacement : Activator, IInteractable {

        public static UnityEvent<bool> onCollect = new UnityEvent<bool>();

        [SerializeField] private string _correctName;
        [SerializeField] private Transform _placementPos;
        private GameObject _heavyObjectCurrent;
        private Collider _collider;
        [SerializeField] private GameObject _interactPopup;

        private InteractableFeedbacks _interactableFeedbacks;
        public InteractableFeedbacks InteratctableFeedbacks { get { return _interactableFeedbacks; } }

        private void Awake() {
            if (_placementPos) {
                if (_placementPos.childCount > 0) _heavyObjectCurrent = _placementPos.GetChild(0).gameObject;
            }
            else Debug.LogError($"{name} has no _placementPos assigned!");
            if (!TryGetComponent<InteractableFeedbacks>(out _interactableFeedbacks)) Debug.LogError($"'{name}' has no InteractableFeedbacks attached to!");
            if (!TryGetComponent<Collider>(out _collider)) Debug.LogError($"'{name}' has no Collider attached to!");
            onCollect.AddListener((isCollecting) => {
                bool shouldShow = _heavyObjectCurrent ^ isCollecting;
                _collider.enabled = shouldShow;
                _interactPopup.SetActive(shouldShow);
            });
        }

        private void Start() {
            _collider.enabled = _heavyObjectCurrent;
            _interactPopup.SetActive(_heavyObjectCurrent);
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
                PlayerActions.Instance.HeavyObjectHold(_heavyObjectCurrent);
                _heavyObjectCurrent = null;
                IsActive = false;

                PlayerMovement.Instance.AllowRun(false);
                PlayerMovement.Instance.AllowCrouch(false);
                PlayerStress.Instance.onFail.AddListener(RemovePlayerObject);
                PlayerAnimation.Instance.HeavyHold(true);
                onCollect.Invoke(true);
                return true;
            }
            else Debug.LogError($"Could not Collect from '{name}': Place empty.");
            return false;
        }

        public bool TryPlace() {
            if (!_heavyObjectCurrent) {
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
                return true;
            }
            else Debug.LogError($"Could not Place to '{name}': Place not empty.");
            return false;
        }

        private void CheckForActivation() {
            if (_heavyObjectCurrent?.name == _correctName) {
                IsActive = true;
                onActivate.Invoke();
            }
        }

        private void RemovePlayerObject() {
            PlayerActions.Instance.HeavyObjectRelease().transform.parent = _placementPos;
            PlayerMovement.Instance.AllowRun(true);
            PlayerMovement.Instance.AllowCrouch(true);
            PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
            PlayerAnimation.Instance.HeavyHold(false);
        }

    }
}
