using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class HeavyObjectPlacement : Activator, IInteractable {

        [SerializeField] private string _correctName;
        [SerializeField] private Transform _placementPos;
        private GameObject _heavyObjectCurrent;

        private InteractableFeedbacks _interactableFeedbacks;
        public InteractableFeedbacks InteratctableFeedbacks { get { return _interactableFeedbacks; } }

        private const string HEAVY_OBJECT_BLOCK_KEY = "HeavyObject";

        private void Awake() {
            if (_placementPos) {
                if (_placementPos.childCount > 0) _heavyObjectCurrent = _placementPos.GetChild(0).gameObject;
            }
            else Debug.LogError($"{name} has no _placementPos assigned!");
            if (!TryGetComponent<InteractableFeedbacks>(out _interactableFeedbacks)) Debug.LogError($"'{name}' has no InteractableFeedbacks attached to!");
        }

        public PlayerActions.InteractAnimation Interact() {
            if (PlayerActions.Instance.IsHoldingHeavyObject) {
                if (TryPlace()) return PlayerActions.InteractAnimation.EnterLocker;
            }
            else if (TryCollect()) return PlayerActions.InteractAnimation.EnterTrash;
            return PlayerActions.InteractAnimation.Default;
        }

        public bool TryCollect() {
            if (_heavyObjectCurrent) {
                PlayerActions.Instance.HeavyObjectHold(_heavyObjectCurrent);
                _heavyObjectCurrent = null;
                IsActive = false;

                PlayerMovement.Instance.AllowRun(false);
                PlayerStress.Instance.onFail.AddListener(RemovePlayerObject);
                return true;
            }
            else Debug.Log($"Could not Collect from '{name}': Place empty.");
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
                PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
                return true;
            }
            else Debug.Log($"Could not Place to '{name}': Place not empty.");
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
            PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
        }

    }
}
