using Ivayami.Player;
using Ivayami.Puzzle;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.UI {
    public class Pause : MonoSingleton<Pause> {

        [Header("Events")]

        public UnityEvent onPause = new UnityEvent();
        public UnityEvent onUnpause = new UnityEvent();

        [Header("Inputs")]

        [SerializeField] private InputActionReference _pauseInput;
        [SerializeField] private InputActionReference _unpauseInput;

        [HideInInspector] public bool canPause = false;

        private void Start() {
            _pauseInput.action.started += (callBackContext) => PauseGame(true);
            _unpauseInput.action.started += (callBackContext) => PauseGame(false);
            onPause.AddListener(() => ReturnAction.Instance.Set(UnpauseOnBack));
        }

        public void PauseGame(bool isPausing) {
            if (canPause) {
                (isPausing ? onPause : onUnpause)?.Invoke();
                PlayerActions.Instance.ChangeInputMap(isPausing ? "Menu" : "Player");

                Logger.Log(LogType.UI, $"Game Pause: {isPausing}");
            }
            else Logger.Log(LogType.UI, "Game Cannot Pause");
        }

        private void UnpauseOnBack() {
            if (InputCallbacks.Instance.IsGamepad) PauseGame(false);

        }

    }
}