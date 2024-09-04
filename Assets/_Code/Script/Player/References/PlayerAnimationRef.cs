using UnityEngine;

namespace Ivayami.Player {
    public class PlayerAnimationRef : MonoBehaviour {

        public void GoToIdle() {
            PlayerAnimation.Instance.GoToIdle();
        }

        public void GetUp() {
            PlayerAnimation.Instance.GetUp();
        }

    }
}