using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Player;
using Ivayami.Puzzle;
using Ivayami.Scene;
using Ivayami.Dialogue;

namespace Ivayami.UI {
    public class Pause : MonoSingleton<Pause> {

        [Header("Events")]

        public UnityEvent onPause = new UnityEvent();
        public UnityEvent onUnpause = new UnityEvent();

        [Header("Inputs")]

        [SerializeField] private InputActionReference _pauseInput;
        [SerializeField] private InputActionReference _unpauseInput;

        [HideInInspector] public bool canPause = false;
        public bool Paused { get; private set; } = false;

        private void Start() {
            _pauseInput.action.started += (callBackContext) => PauseGame(true);
            _unpauseInput.action.started += (callBackContext) => PauseGame(false);

            onPause.AddListener(() => ReturnAction.Instance.Set(UnpauseOnBack));
            PlayerStress.Instance.onFail.AddListener(UnpauseIfPaused);
            PlayerStress.Instance.onFail.AddListener(() => canPause = false);
            PlayerStress.Instance.onFailFade.AddListener(() => canPause = true);
            SceneController.Instance.OnAllSceneRequestEnd += UnpauseIfPaused;
            DialogueController.Instance.OnDialogeStart += UnpauseIfPaused;
        }

        public void PauseGame(bool isPausing) {
            if (canPause) {
                Paused = isPausing;
                (Paused ? onPause : onUnpause)?.Invoke();
                PlayerActions.Instance.ChangeInputMap(Paused ? "Menu" : "Player");

                Logger.Log(LogType.UI, $"Game Pause: {Paused}");
            }
            else Logger.Log(LogType.UI, "Game Cannot Pause");
        }

        public void UnpauseOnBack() {
            if (InputCallbacks.Instance.IsGamepad) PauseGame(false);

        }

        private void UnpauseIfPaused() {
            if (Paused) PauseGame(false);
        }

    }
}