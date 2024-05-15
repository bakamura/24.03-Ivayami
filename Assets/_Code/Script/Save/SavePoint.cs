using UnityEngine;
using Ivayami.Player;
using Ivayami.Puzzle;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Ivayami.Save {
    public class SavePoint : MonoBehaviour, IInteractable {

        public InteractableHighlight InteratctableHighlight { get; private set; }

        [Header("Save Interact")]

        [SerializeField] private int _pointId;
        [SerializeField] private Transform _playerAnimationPoint;
        public Transform spawnPoint;
        public static Dictionary<int, SavePoint> Points { get; private set; } = new Dictionary<int, SavePoint>();
        public static UnityEvent onSaveGame = new UnityEvent();
        public static UnityEvent onCantSaveGame = new UnityEvent();
        private bool _canSave = true;

        private void Awake() {
            InteratctableHighlight = GetComponent<InteractableHighlight>();
            Points.Add(_pointId, this);
        }

        private void Start() {
            onSaveGame.AddListener(() => PlayerMovement.Instance.transform.position = _playerAnimationPoint.position);
            PlayerStress.Instance.onStressChange.AddListener(stress => _canSave = stress <= 0);
        }

        public void Interact() {
            if (_canSave) {
                onSaveGame?.Invoke();

                Logger.Log(LogType.Save, "SavePoint Call Save");
            }
            else {
                onCantSaveGame?.Invoke();

                Logger.Log(LogType.Save, "SavePoint Cannot Save");
            }
        }
    }
}