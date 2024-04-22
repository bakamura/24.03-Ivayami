using Ivayami.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.UI {
    public class Pause : MonoSingleton<Pause> {

        [Header("Events")]

        public UnityEvent<bool> onPause = new UnityEvent<bool>();

        [Header("Menu")]

        [SerializeField] private Menu _hud;
        [SerializeField] private Menu _pause;
        [SerializeField] private MenuGroup _menuGroup;

        [Header("Inputs")]

        [SerializeField] private InputActionReference _pauseInput;
        [SerializeField] private InputActionReference _unpauseInput;

        private void Start() {
            _pauseInput.action.started += (callBackContext) => PauseGame(true);
            _unpauseInput.action.started += (callBackContext) => PauseGame(false);
            onPause.AddListener((pausing) => {
                _menuGroup.CloseCurrentThenOpen(pausing ? _pause : _hud);
                PlayerActions.Instance.ChangeInputMap(pausing ? "Menu" : "Player");
            });
        }

        public void PauseGame(bool isPausing) {
            onPause?.Invoke(isPausing);

            Logger.Log(LogType.UI, $"Game Pause: {isPausing}");
        }

    }
}