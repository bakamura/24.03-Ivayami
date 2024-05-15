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
        private float _stressMin;
        private float _stressCurrent;
        [SerializeField] private float _stressRelieveDelay;
        private float _stressRelieveDelayTimer;
        [SerializeField, Tooltip("In seconds")] private float _stressRelieveRate;

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

        public void AddStress(float amount) {
            if (!_failState) {
                _stressCurrent += amount;
                onStressChange.Invoke(_stressCurrent);

                float stressRelieveDelayTimerLast = _stressRelieveDelayTimer;
                _stressRelieveDelayTimer = 0;
                if (_stressRelieveRoutine == null) _stressRelieveRoutine = StartCoroutine(StressRelieveAuto());
                else {
                    if (stressRelieveDelayTimerLast >= _stressRelieveDelay) {
                        StopCoroutine(_stressRelieveRoutine);
                        _stressRelieveRoutine = StartCoroutine(StressRelieveAuto());

                        Logger.Log(LogType.Player, $"Interrupted Relieving Stress");
                    }
                }


                Logger.Log(LogType.Player, $"Stress Meter: {_stressCurrent}/{_stressMax}");
            }
        }

        private IEnumerator StressRelieveAuto() {
            while (_stressRelieveDelayTimer < _stressRelieveDelay) {
                _stressRelieveDelayTimer += Time.deltaTime;

                yield return null;
            }

            Logger.Log(LogType.Player, $"Started Relieving Stress");

            while (_stressCurrent > _stressMin) {
                _stressCurrent -= _stressRelieveRate * Time.deltaTime;
                onStressChange.Invoke(_stressCurrent);

                yield return null;
            }

            Logger.Log(LogType.Player, $"Ended Relieving Stress");

            _stressCurrent = _stressMin;
            _stressRelieveRoutine = null;
        }

        private void FailStateCheck(float stressCurrent) {
            if (!_failState && stressCurrent >= _stressMax) {
                _failState = true;
                onFailState.Invoke();

                Logger.Log(LogType.Player, $"Player Fail State");
            }
        }

        public void SetStressMin(float stressMin) {
            _stressMin = stressMin;
            if (_stressCurrent > _stressMin && _stressRelieveRoutine == null) {
                _stressRelieveDelayTimer = 0;
                _stressRelieveRoutine = StartCoroutine(StressRelieveAuto());
            }
        }

        private IEnumerator DelayToRespawn() {
            PlayerMovement.Instance.ToggleMovement(false);
            yield return _restartWait;

            SceneTransition.Instance.Menu.Close();

            yield return new WaitForSeconds(SceneTransition.Instance.Menu.TransitionDuration);

            transform.position = SavePoint.Points[SaveSystem.Instance.Progress.pointId].transform.position;
            PlayerMovement.Instance.ToggleMovement(true);
            SceneTransition.Instance.Menu.Open();
        }

    }
}