using UnityEngine;

namespace Ivayami.Player.Ability {
    public class CallFriend : PlayerAbility {

        public override void AbilityStart() {


            Logger.Log(LogType.Player, $"Call Friend");
        }

        public override void AbilityEnd() { }

    }
}