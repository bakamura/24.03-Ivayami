using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using Ivayami.Puzzle;
using Ivayami.Scene;

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

        private void Awake() {
            InteratctableFeedbacks = GetComponent<InteractableFeedbacks>();
            UpdatePointsDictionary(_pointId, this);
        }

        private void Start() {
            onSaveGame.AddListener(() => {
                if (_playerAnimationPoint) PlayerMovement.Instance.transform.position = _playerAnimationPoint.position;
            });
            PlayerStress.Instance.onStressChange.AddListener(stress => _canSave = stress <= 0);
        }

        private void Save() {
            SaveSystem.Instance.Progress.pointId = _pointId;
            onSaveGame?.Invoke();

            Logger.Log(LogType.Save, "SavePoint Call Save");
        }

        public PlayerActions.InteractAnimation Interact() {
            if (!_canSave) {
                onCantSaveGame?.Invoke();

                Logger.Log(LogType.Save, "SavePoint Cannot Save");
                return PlayerActions.InteractAnimation.Default;
            }
            Save();
            return PlayerActions.InteractAnimation.Seat;
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

    }
}