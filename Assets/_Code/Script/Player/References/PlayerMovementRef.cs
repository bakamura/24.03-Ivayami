using UnityEngine;

namespace Ivayami.Player {
    public class PlayerMovementRef : MonoBehaviour {

        public void AllowPlayerRun(bool allow) {
            PlayerMovement.Instance.ToggleMovement(allow);
        }

        public void BlockMovementFor(float seconds) {
            PlayerMovement.Instance.BlockMovementFor(seconds);
        }

    }
}
