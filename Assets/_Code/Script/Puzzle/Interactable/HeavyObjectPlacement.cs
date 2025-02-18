using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Puzzle {
    public class HeavyObjectPlacement : MonoBehaviour, IInteractable {

        private GameObject _heavyObjectCurrent;
        [SerializeField] private Transform _placementPos;

        private InteractableFeedbacks _interactableFeedbacks;
        public InteractableFeedbacks InteratctableFeedbacks { get { return _interactableFeedbacks; } }

        private const string HEAVY_OBJECT_BLOCK_KEY = "HeavyObject";

        private void Awake() {
            if (!_placementPos) Debug.LogError($"{name} has no _placementPos assigned!");
            if (!TryGetComponent<InteractableFeedbacks>(out _interactableFeedbacks)) Debug.LogError($"'{name}' has no InteractableFeedbacks attached to!");
        }

        public PlayerActions.InteractAnimation Interact() {
            if (PlayerActions.Instance.IsHoldingHeavyObject) {
                if (TryCollect()) return PlayerActions.InteractAnimation.EnterTrash;
            }
            else if (TryPlace()) return PlayerActions.InteractAnimation.EnterLocker;
            return PlayerActions.InteractAnimation.Default;
        }

        public bool TryCollect() {
            if (_heavyObjectCurrent) {
                PlayerActions.Instance.HeavyObjectHold(_heavyObjectCurrent);
                _heavyObjectCurrent = null;

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

                PlayerMovement.Instance.AllowRun(true);
                PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
                return true;
            }
            else Debug.Log($"Could not Place to '{name}': Place not empty.");
            return false;
        }

        private void RemovePlayerObject() {
            _heavyObjectCurrent.transform.parent = _placementPos;
            PlayerMovement.Instance.AllowRun(true);
            PlayerActions.Instance.HeavyObjectRelease();
            PlayerStress.Instance.onFail.RemoveListener(RemovePlayerObject);
        }

    }
}
