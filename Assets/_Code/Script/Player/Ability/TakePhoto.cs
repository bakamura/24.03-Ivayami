using Cinemachine;
using UnityEngine;

namespace Ivayami.Player.Ability {
    public class TakePhoto : PlayerAbility {

        private bool _cameraOpen;
        private CinemachineFreeLook _cameraFreelook;
        [SerializeField] private int _priorityWhenOpen;

        private void Awake() {
            _cameraFreelook = FindObjectOfType<CinemachineFreeLook>();
        }

        public override void AbilityStart() {
            _cameraFreelook.Priority = _cameraOpen ? _priorityWhenOpen : -1;
            // Call camera filter in postprocessing

            Logger.Log(LogType.Player, $"Camera {(_cameraOpen ? "Open" : "Close")}");
        }

        public override void AbilityEnd() { }

    }
}