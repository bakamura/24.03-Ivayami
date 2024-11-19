using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;

namespace Ivayami.Scene {
    public class PlayerLockInput : MonoBehaviour {

        public void LockInput() {
            Debug.LogWarning($"{name} is using PlayerLockInput, please remove it!");
            PlayerActions.Instance.ChangeInputMap(null);
        }

        public void UnlockInput() {
            Debug.LogWarning($"{name} is using PlayerLockInput, please remove it!");
            PlayerActions.Instance.ChangeInputMap("Player");
        }

        public void ChangeInputToUI() {
            Debug.LogWarning($"{name} is using PlayerLockInput, please remove it!");
            PlayerActions.Instance.ChangeInputMap("Menu");
            Pause.Instance.ToggleCanPause(nameof(PlayerLockInput), false);
        }

        public void RemovePauseBlock()
        {
            Pause.Instance.ToggleCanPause(nameof(PlayerLockInput), true);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            Debug.LogWarning($"{name} is using PlayerLockInput, please remove it!");
        }
#endif

    }
}