using UnityEngine;
using Ivayami.Player;
using Ivayami.Puzzle;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Ivayami.Save {
    public class SavePoint : MonoBehaviour, IInteractable {

        public InteractableFeedbacks InteratctableHighlight { get; private set; }

        [Header("Save Interact")]

        [SerializeField] private int _pointId;
        [SerializeField] private Transform _playerAnimationPoint;
        public Transform spawnPoint;
        public static Dictionary<int, SavePoint> Points { get; private set; } = new Dictionary<int, SavePoint>();
        public static UnityEvent onSaveGame = new UnityEvent();
        public static UnityEvent onCantSaveGame = new UnityEvent();
        private bool _canSave = true;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableFeedbacks>();
            UpdatePointsDictionary(_pointId, this);
        }

        private void Start() {
            onSaveGame.AddListener(() =>
            {
                if (_playerAnimationPoint)
                {
                    PlayerMovement.Instance.transform.position = _playerAnimationPoint.position;
                }
            });
            PlayerStress.Instance.onStressChange.AddListener(stress => _canSave = stress <= 0);
        }

        private void Save() {
            SaveSystem.Instance.Progress.pointId = _pointId;
            onSaveGame?.Invoke();

            Logger.Log(LogType.Save, "SavePoint Call Save");
        }

        public PlayerActions.InteractAnimation Interact() {
            if (_canSave) Save();
            else {
                onCantSaveGame?.Invoke();

                Logger.Log(LogType.Save, "SavePoint Cannot Save");
            }
            return PlayerActions.InteractAnimation.Default;
        }

        public void ForceSave() {
            Save();
        }

        private void UpdatePointsDictionary(int key, SavePoint value) {
            if (!Points.ContainsKey(key))
            {
                Points.Add(key, value);
            }
            else Points[key] = value;
        }

    }
}