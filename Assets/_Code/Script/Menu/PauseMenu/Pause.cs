using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Paranapiacaba.UI {
    public class Pause : MonoSingleton<Pause> {

        [Header("Events")]

        public UnityEvent<bool> onPause = new UnityEvent<bool>();

        [Header("Menu")]

        [SerializeField] private Menu _hud;
        [SerializeField] private Menu _pause;
        private MenuGroup _menuGroup;

        [Header("Inputs")]

        [SerializeField] private InputActionReference _pauseInput;
        [SerializeField] private InputActionReference _unpauseInput;

        private void Start() {
            _pauseInput.action.started += (callBackContext) => PauseGame(true);
            _unpauseInput.action.started += (callBackContext) => PauseGame(true);
            _menuGroup = new MenuGroup(_hud, -1);
            onPause.AddListener((pausing) => _menuGroup.CloseCurrentThenOpen(pausing ? _pause : _hud));
        }

        public void PauseGame(bool isPausing) {
            onPause?.Invoke(isPausing);
        }

    }
}