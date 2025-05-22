using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Player;
using Ivayami.Puzzle;
using Ivayami.Scene;
using Ivayami.UI;
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
        public static UnityEvent onSaveGameWithAnimation = new UnityEvent();
        public static UnityEvent onSaveSequenceEnd = new UnityEvent();
        private bool _canSave = true;

        [SerializeField] private InputActionReference _movementInput;
        [SerializeField] private GameObject _interactableIcon;
        [SerializeField] private Dialogue.Dialogue _preventSaveDialogue;
        [SerializeField, ReadOnly] private string _dialogueName;

        [SerializeField, Min(0)] private float _delayFadeOut;
        private WaitForSeconds _delayFadeOutWait;
        [SerializeField, Min(0)] private float _delayUnlockMovement;
        private WaitForSeconds _delayUnlockMovementWait;

        public const string BLOCKER_KEY = "SavePointBlocker";

        private void Awake() {
            InteratctableFeedbacks = GetComponent<InteractableFeedbacks>();
            UpdatePointsDictionary(_pointId, this);

            _delayFadeOutWait = new WaitForSeconds(_delayFadeOut);
            _delayUnlockMovementWait = new WaitForSeconds(_delayUnlockMovement);
        }

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(stress => _canSave = stress <= PlayerStress.Instance.StressRelieveMinValue);
            if (_preventSaveDialogue)
            {
                _dialogueName = _preventSaveDialogue.name;
                Resources.UnloadAsset(_preventSaveDialogue);
            }
        }

        private void Save() {
            SaveSystem.Instance.Progress.pointId = _pointId;
            onSaveGame?.Invoke();
            
            PlayerMovement.Instance.ToggleMovement(BLOCKER_KEY, false);
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, false);
            _interactableIcon.SetActive(false);
            SceneTransition.Instance.OnOpenEnd.AddListener(OnSaveFadeOutEnd);
            SceneTransition.Instance.Open();

            Logger.Log(LogType.Save, $"SavePoint [{_pointId}] Call Save");
        }

        public PlayerActions.InteractAnimation Interact() {
            if (!_canSave) {
                onCantSaveGame?.Invoke();
                DialogueController.Instance.StartDialogue(_dialogueName, false);

                Logger.Log(LogType.Save, "SavePoint Cannot Save");
                return PlayerActions.InteractAnimation.Default;
            }
            onSaveGameWithAnimation?.Invoke();
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
            SceneTransition.Instance.OnOpenEnd.RemoveListener(OnSaveFadeOutEnd);

            if (_playerAnimationPoint) {
                PlayerMovement.Instance.transform.position = _playerAnimationPoint.position;
                PlayerMovement.Instance.SetTargetAngle(_playerAnimationPoint.eulerAngles.y);
            }
            else Debug.LogWarning($"Save point '{name}' has no _playerAnimationPoint referenced");
            PlayerStress.Instance.AddStress(-1000);

            yield return _delayFadeOutWait;

            SceneTransition.Instance.Close();
            PlayerAnimation.Instance.Sit();
            InfoUpdateIndicator.Instance.DisplaySaved();
            _movementInput.action.performed += OnSaveFadeInEnd;
        }

        private void OnSaveFadeInEnd(InputAction.CallbackContext context) {
            _movementInput.action.performed -= OnSaveFadeInEnd;

            PlayerAnimation.Instance.GetUpSit();
            Pause.Instance.ToggleCanPause(BLOCKER_KEY, true);
            StartCoroutine(OnSaveFadeInEndRoutine());
        }
        
        private IEnumerator OnSaveFadeInEndRoutine() {
            yield return _delayUnlockMovementWait;

            _interactableIcon.SetActive(true);
            PlayerMovement.Instance.ToggleMovement(BLOCKER_KEY, true);
            onSaveSequenceEnd?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_preventSaveDialogue)
            {
                _dialogueName = _preventSaveDialogue.name;
            }
        }
#endif
    }
}