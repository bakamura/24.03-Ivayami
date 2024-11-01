using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Save;
using Ivayami.Scene;
using Ivayami.UI;

namespace Ivayami.Player {
    public class PlayerStress : MonoSingleton<PlayerStress> {

        [Header("Events")]

        public UnityEvent<float> onStressChange = new UnityEvent<float>();
        public UnityEvent onFail = new UnityEvent();
        public UnityEvent onFailFade = new UnityEvent();

        [Header("Stress")]

        [SerializeField] private float _stressMax;
        private float _stressCurrent;
        [SerializeField] private float _stressRelieveDelay;
        private float _stressRelieveDelayTimer;
        private bool _pauseStressRelieve = false;

        public float MaxStress => _stressMax;

        [Header("Fail")]

        [SerializeField] private float _restartDelay;
        private WaitForSeconds _restartWait;
        private bool _failState = false;
        private bool _overrideFailLoad = false;

        [Header("Cache")]

        private const string FAIL_BLOCK_KEY = "FailState";
        private bool _isAutoRegenActive = true;

        private void Start() {
            Pause.Instance.onPause.AddListener(() => _pauseStressRelieve = true);
            Pause.Instance.onUnpause.AddListener(() => _pauseStressRelieve = false);
            onStressChange.AddListener(FailStateCheck);
            onFail.AddListener(() => StartCoroutine(DelayToRespawn()));
            onFailFade.AddListener(ResetStress);
            onFail.AddListener(() => Pause.Instance.ToggleCanPause(FAIL_BLOCK_KEY, false));
            onFailFade.AddListener(() => Pause.Instance.ToggleCanPause(FAIL_BLOCK_KEY, true));
            _restartWait = new WaitForSeconds(_restartDelay);

            Logger.Log(LogType.Player, $"{typeof(PlayerStress).Name} Initialized");
        }

        private void Update() {
#if UNITY_EDITOR
            if (!_isAutoRegenActive) return;
#endif
            if (!_pauseStressRelieve) {
                if (_stressRelieveDelayTimer > 0) _stressRelieveDelayTimer -= Time.deltaTime;
                else if (_stressCurrent > 20f) RelieveStressAuto();
            }
        }

        public void AddStress(float amount, float capValue = -1) {
            if (!_failState && _stressCurrent < (capValue >= 0 ? capValue : _stressMax)) {
                _stressCurrent = Mathf.Clamp(_stressCurrent + amount, 0, capValue >= 0 ? capValue : _stressMax);
                onStressChange.Invoke(_stressCurrent);
                _stressRelieveDelayTimer = _stressRelieveDelay;

                Logger.Log(LogType.Player, $"Stress Meter: {_stressCurrent}/{_stressMax}");
            }
            else if (_stressCurrent == capValue) _stressRelieveDelayTimer = _stressRelieveDelay;
        }

        private void RelieveStressAuto() {
            _stressCurrent += StressRelieveFormula(_stressCurrent) * Time.deltaTime;
            onStressChange.Invoke(_stressCurrent);
        }

        private float StressRelieveFormula(float intake) {
            if (intake > 20) return -0.0001f * Mathf.Pow(intake + 65f, 2f); // Tweak / Modularize values later
            else return 0;
        }

        private void FailStateCheck(float stressCurrent) {
            if (!_failState && stressCurrent >= _stressMax) {
                _failState = true;
                onFail.Invoke();

                Logger.Log(LogType.Player, $"Player Fail State");
            }
        }

        private void ResetStress() {
            _failState = false;
            _stressCurrent = 0;
            onStressChange.Invoke(_stressCurrent);
            PlayerAnimation.Instance.GoToIdle();
            PlayerMovement.Instance.ToggleMovement(FAIL_BLOCK_KEY, true);
        }

        public void OverrideFailLoad() {
            _overrideFailLoad = true;
        }

        private IEnumerator DelayToRespawn() {
            PlayerMovement.Instance.ToggleMovement(FAIL_BLOCK_KEY, false);

            yield return _restartWait;

            SceneTransition.Instance.OnOpenEnd.AddListener(RespawnFailFade);
            SceneTransition.Instance.Open();
        }

        private void RespawnFailFade() {
            onFailFade.Invoke();

            if (_overrideFailLoad) _overrideFailLoad = false;
            else SaveSystem.Instance.LoadProgress(SaveSystem.Instance.Progress.id, () => SceneController.Instance.UnloadAllScenes(ReloadAndReset));
            SceneTransition.Instance.OnOpenEnd.RemoveListener(RespawnFailFade);
        }

        private void ReloadAndReset() {
            UnityEvent onSceneLoaded = new UnityEvent();
            onSceneLoaded.AddListener(() => SavePoint.Points[SaveSystem.Instance.Progress.pointId].SpawnPoint.Teleport());
            SceneController.Instance.LoadScene("BaseTerrain", onSceneLoaded);
            SceneController.Instance.OnAllSceneRequestEnd -= ReloadAndReset;
            PlayerInventory.Instance.LoadInventory(SaveSystem.Instance.Progress.inventory);
        }

        public void UpdateAutoRegenerateStress(bool isActive)
        {
            if (!IngameDebugConsole.DebugLogManager.Instance) return;
            _isAutoRegenActive = isActive;
        }
    }
}