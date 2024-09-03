using UnityEngine;

namespace Ivayami.Player {
    public class PlayerWalkRef : MonoBehaviour {

        public void AlloPlayerRun(bool allow) {
            PlayerMovement.Instance.ToggleMovement(allow);
        }

    }
}
