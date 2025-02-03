using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using Ivayami.Puzzle;
using Ivayami.Scene;
using Ivayami.UI;
using System.Collections;
using Ivayami.Dialogue;

namespace Ivayami.Save {
    public class SavePoint : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableFeedbacks { get; private set; }

        [Header("Save Interact")]

        [SerializeField] private int _pointId;
        [SerializeField] private Transform _playerAnimationPoint;
        [field: SerializeField] public ManualTeleporter SpawnPoint { get; private set; }
        public static Dictionary<int, SavePoint> Points { get; private set; } = new Dictionary<int, SavePoint>();
        public static UnityEvent onSaveGame = new UnityEvent();
        public static UnityEvent onCantSaveGame = new UnityEvent();
        private bool _canSave = true;

        private DialogueTrigger _dialogueTrigger;

        [SerializeField, Min(0)] private float _delayFadeOut;
        private WaitForSeconds _delayFadeOutWait;
        [SerializeField, Min(0)] private float _delayUnlockMovement;
        private WaitForSeconds _delayUnlockMovementWait;

        private const string BLOCK_KEY = "SavePointBlocker";

        private void Awake() {
            InteratctableFeedbacks = GetComponent<InteractableFeedbacks>();
            UpdatePointsDictionary(_pointId, this);

            _dialogueTrigger = GetComponent<DialogueTrigger>();
            _delayFadeOutWait = new WaitForSeconds(_delayFadeOut);
            _delayUnlockMovementWait = new WaitForSeconds(_delayUnlockMovement);
        }

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(stress => _canSave = stress <= PlayerStress.Instance.StressRelieveMinValue);
        }

        private void Save() {
            SaveSystem.Instance.Progress.pointId = _pointId;
            onSaveGame?.Invoke();

            PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false);
            Pause.Instance.ToggleCanPause(BLOCK_KEY, false);
            SceneTransition.Instance.OnOpenEnd.AddListener(OnSaveFadeOutEnd);
            SceneTransition.Instance.Open();

            Logger.Log(LogType.Save, $"SavePoint [{_pointId}] Call Save");
        }

        public PlayerActions.InteractAnimation Interact() {
            if (!_canSave) {
                onCantSaveGame?.Invoke();
                _dialogueTrigger.StartDialogue();
                Debug.Log("Cant Save");
                Logger.Log(LogType.Save, "SavePoint Cannot Save");
                return PlayerActions.InteractAnimation.Default;
            }
            Debug.Log("Save");
            Save();
            return PlayerActions.InteractAnimation.Default;
        }

        public void ForceSave() {
            Save();
        }

        private void UpdatePointsDictionary(int key, SavePoint value) {
            if (!Points.ContainsKey(key)) Points.Add(key, value);
            else {
                if (Points[key] != null) Debug.LogWarning($"It's possible there is a duplicate instance of SavePoint with ID [{key}] {Points[key].name} substituted by {value.name}");
                Points[key] = value;
            }
        }

        private void OnSaveFadeOutEnd() {
            StartCoroutine(OnSaveFadeOutEndRoutine());
        }
        
        private IEnumerator OnSaveFadeOutEndRoutine() {
            if (_playerAnimationPoint) {
                PlayerMovement.Instance.transform.position = _playerAnimationPoint.position;
                PlayerMovement.Instance.SetTargetAngle(_playerAnimationPoint.eulerAngles.y);
            }
            else Debug.LogWarning($"Save point '{name}' has no _playerAnimationPoint referenced");
            
            yield return _delayFadeOutWait;

            PlayerAnimation.Instance.GetUpSit();
            SceneTransition.Instance.OnOpenEnd.RemoveListener(OnSaveFadeOutEnd);
            SceneTransition.Instance.OnCloseEnd.AddListener(OnSaveFadeInEnd);
            SceneTransition.Instance.Close();
        }

        private void OnSaveFadeInEnd() {
            Pause.Instance.ToggleCanPause(BLOCK_KEY, true);
            SceneTransition.Instance.OnCloseEnd.RemoveListener(OnSaveFadeInEnd);
            StartCoroutine(OnSaveFadeInEndRoutine());
        }
        
        private IEnumerator OnSaveFadeInEndRoutine() {
            yield return _delayUnlockMovementWait;

            PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, true);
        }

    }
}