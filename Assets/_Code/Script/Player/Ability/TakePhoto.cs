using UnityEngine;

namespace Ivayami.Player.Ability {
    public class TakePhoto : PlayerAbility {

        private bool _cameraOpen;

        public override void AbilityStart() {


            Logger.Log(LogType.Player, $"Camera {(_cameraOpen ? "Open" : "Close")}");
        }

        public override void AbilityEnd() { }

    }
}