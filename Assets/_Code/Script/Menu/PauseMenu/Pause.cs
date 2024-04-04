using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Paranapiacaba.UI {
    public class Pause : MonoSingleton<Pause> {

        [Header("Events")]

        public UnityEvent<bool> onPause = new UnityEvent<bool>();

        [Header("Inputs")]

        [SerializeField] private InputActionReference _pauseInput;
        [SerializeField] private InputActionReference _unpauseInput;

        private void Start() {
            _pauseInput.action.started += (callBackContext) => PauseGame(true);
            _unpauseInput.action.started += (callBackContext) => PauseGame(true);
        }

        public void PauseGame(bool isPausing) {
            onPause?.Invoke(isPausing);
        }

    }
}