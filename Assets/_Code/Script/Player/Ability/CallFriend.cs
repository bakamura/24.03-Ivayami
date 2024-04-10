using UnityEngine;

namespace Paranapiacaba.Player.Ability {
    public class CallFriend : PlayerAbility {

        public override void AbilityStart() {


            Logger.Log(LogType.Player, $"Call Friend");
        }

        public override void AbilityEnd() { }

    }
}