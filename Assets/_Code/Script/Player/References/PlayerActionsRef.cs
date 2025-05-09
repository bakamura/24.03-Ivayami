using UnityEngine;

namespace Ivayami.Player {
    public class PlayerActionsRef : MonoBehaviour {

        public void ChangeInputMap(string map) {
            PlayerActions.Instance.ChangeInputMap(map);
        }

    }
}
