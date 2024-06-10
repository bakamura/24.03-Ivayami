using UnityEngine;
using UnityEngine.InputSystem;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.Save;

namespace Ivayami.Scene {
    public class EnterGameHelper : MonoBehaviour {

        [Header("Input Stopping")]

        [SerializeField] private InputActionReference _pauseInput;

        private void Awake() {
            _pauseInput.action.Disable();
            SceneController.Instance.OnAllSceneRequestEnd = () => SceneController.Instance.OnAllSceneRequestEnd = EnablePlayerInput;
            DontDestroyOnLoad(gameObject);
        }

        private void EnablePlayerInput() {
            Debug.Log("EnablePlayerInput");
            _pauseInput.action.Enable();
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerMovement.Instance.ToggleMovement(true);
            PlayerMovement.Instance.transform.position = SavePoint.Points[SaveSystem.Instance.Progress.pointId].spawnPoint.position;
            SceneTransition.Instance.Menu.Open();
            SceneController.Instance.OnAllSceneRequestEnd = null;

            Logger.Log(LogType.UI, $"EnablePlayerInput callback");
            Destroy(gameObject);
        }

    }
}
