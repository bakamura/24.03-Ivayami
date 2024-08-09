using Ivayami.Save;
using Ivayami.Scene;
using Ivayami.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Player {
    public class PlayerStress : MonoSingleton<PlayerStress> {

        [Header("Events")]

        public UnityEvent<float> onStressChange = new UnityEvent<float>();
        public UnityEvent onFailState = new UnityEvent();

        [Header("Stress")]

        [SerializeField] private float _stressMax;
        private float _stressCurrent;
        [SerializeField] private float _stressRelieveDelay;
        private float _stressRelieveDelayTimer;

        [Header("Fail")]

        [SerializeField] private float _restartDelay;
        private WaitForSeconds _restartWait;
        private bool _failState = false;

        [Header("Cache")]

        private Coroutine _stressRelieveRoutine;

        private void Start() {
            onStressChange.AddListener(FailStateCheck);
            onFailState.AddListener(() => StartCoroutine(DelayToRespawn()));
            _restartWait = new WaitForSeconds(_restartDelay);

            Logger.Log(LogType.Player, $"{typeof(PlayerStress).Name} Initialized");
        }

        private void Update() {
            if (_stressCurrent > 20f) RelieveStressAuto();
        }

        public void AddStress(float amount, float capValue = -1) {
            if (!_failState) {
                _stressCurrent = Mathf.Clamp(_stressCurrent + amount, 0, capValue >= 0 ? capValue : _stressMax);
                onStressChange.Invoke(_stressCurrent);

                Logger.Log(LogType.Player, $"Stress Meter: {_stressCurrent}/{_stressMax}");
            }
        }

        private void RelieveStressAuto() {
            _stressCurrent += StressRelieveFormula(_stressCurrent);
            onStressChange.Invoke(_stressCurrent);
        }

        private float StressRelieveFormula(float intake) {
            if (intake > 20) return -0.0001f * Mathf.Pow(intake + 65f, 2f); // Tweak / Modularize values later
            else return 0;
        }

        private void FailStateCheck(float stressCurrent) {
            if (!_failState && stressCurrent >= _stressMax) {
                _failState = true;
                onFailState.Invoke();

                Logger.Log(LogType.Player, $"Player Fail State");
            }
        }

        private IEnumerator DelayToRespawn() {
            PlayerMovement.Instance.ToggleMovement(false);

            yield return _restartWait;

            SceneTransition.Instance.Menu.Close();

            yield return new WaitForSeconds(SceneTransition.Instance.Menu.TransitionDuration);

            SceneController.Instance.UnloadAllScenes(HandleUnloadAllScenes);
        }

        private void HandleUnloadAllScenes() {
            SceneController.Instance.OnAllSceneRequestEnd -= HandleUnloadAllScenes;
            UnityEvent onSceneLoaded = new UnityEvent();
            onSceneLoaded.AddListener(UnlockPlayer);
            SceneController.Instance.StartLoad("BaseTerrain", onSceneLoaded);
        }

        private void UnlockPlayer() {
            PlayerMovement.Instance.SetPosition(SavePoint.Points[SaveSystem.Instance.Progress.pointId].transform.position);
            //PlayerMovement.Instance.ToggleMovement(true);
            PlayerAnimation.Instance.GoToIdle();
            _stressCurrent = 0;
            _failState = false;
            onStressChange?.Invoke(_stressCurrent);
        }

    }
}