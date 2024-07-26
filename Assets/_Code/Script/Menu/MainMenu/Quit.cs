using UnityEngine;
using UnityEngine.InputSystem;

namespace Ivayami.UI {
    public class Quit : MonoBehaviour {

        [Header("Input Stopping")]

        [SerializeField] private InputActionReference _pauseInput;

        private void Awake() {
            _pauseInput.action.Disable();
        }

        public void QuitGame() {
            Logger.Log(LogType.UI, $"Quitting Game");
            Application.Quit();
        }

    }
}