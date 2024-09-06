using UnityEngine;

namespace Ivayami.Player {
    public class PlayerMovementRef : MonoBehaviour {

        [SerializeField] private string _blockKey;

        public void AllowRun(bool allow) {
            PlayerMovement.Instance.AllowRun(allow);
        }

        public void ToggleMovement(bool canMove) {
            PlayerMovement.Instance.ToggleMovement(_blockKey, canMove);
        }

        public void BlockMovementFor(float seconds) {
            PlayerMovement.Instance.BlockMovementFor(_blockKey, seconds);
        }

    }
}
