using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Paranapiacaba.Player {
    public class PlayerStress : MonoSingleton<PlayerStress> {

        [Header("Events")]

        public UnityEvent<float> onStressChange = new UnityEvent<float>();
        public UnityEvent onFailState = new UnityEvent();

        [Header("Parameters")]

        [SerializeField] private float _stressMax;
        private float _stressMin;
        private float _stressCurrent;
        [SerializeField] private float _stressRelieveDelay;
        private float _stressRelieveDelayTimer;
        [SerializeField, Tooltip("In seconds")] private float _stressRelieveRate;
        private bool _failState = false;

        [Header("Cache")]

        private Coroutine _stressRelieveRoutine;

        private void Start() {
            onStressChange.AddListener(FailState);

            Logger.Log(LogType.Player, $"{typeof(PlayerStress).Name} Initialized");
        }

        public void AddStress(float amount) {
            if (!_failState) {
                _stressCurrent += amount;
                onStressChange.Invoke(_stressCurrent / _stressMax);
                _stressRelieveDelayTimer = 0;
                if (_stressRelieveRoutine == null) _stressRelieveRoutine = StartCoroutine(StressRelieveAuto());

                Logger.Log(LogType.Player, $"Stress Meter: {_stressCurrent}/{_stressMax}");
            }
        }

        private IEnumerator StressRelieveAuto() {
            while (_stressRelieveDelayTimer < _stressRelieveDelay) _stressRelieveDelayTimer += Time.deltaTime;

            while (_stressCurrent > _stressMin) {
                _stressCurrent -= _stressRelieveRate / Time.deltaTime;

                yield return null;
            }
            _stressCurrent = _stressMin;
            _stressRelieveRoutine = null;
        }

        private void FailState(float stressCurrent) {
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

    }
}