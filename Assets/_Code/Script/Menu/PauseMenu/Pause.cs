using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Player;
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

        private HashSet<string> _pauseBlocker = new HashSet<string>();
        public bool Paused { get; private set; } = false;

        private void Start() {
            _pauseInput.action.started += (callBackContext) => PauseGame(true);
            _unpauseInput.action.started += (callBackContext) => PauseGame(false);

            onPause.AddListener(() => ReturnAction.Instance.Set(UnpauseOnBack));
            PlayerStress.Instance.onFail.AddListener(UnpauseIfPaused);
            SceneController.Instance.OnAllSceneRequestEnd += UnpauseIfPaused;
            DialogueController.Instance.OnDialogeStart += UnpauseIfPaused;
        }

        public void PauseGame(bool isPausing) {
            if (_pauseBlocker.Count <= 0) {
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

        public void ToggleCanPause(string key, bool canPause) {
            if (canPause) {
                if (!_pauseBlocker.Remove(key)) Debug.LogWarning($"'{key}' tried to unlock movement but key isn't blocking");
            }
            else if (!_pauseBlocker.Add(key)) Debug.LogWarning($"'{key}' tried to lock movement but key is already blocking");

            Logger.Log(LogType.UI, $"Puase blockers {(canPause ? "Increase" : "Decrease")} to: {_pauseBlocker.Count}");
        }

        public void RemoveAllBlockers()
        {
            if (!IngameDebugConsole.DebugLogManager.Instance) return;
            _pauseBlocker.Clear();
        }

    }
}