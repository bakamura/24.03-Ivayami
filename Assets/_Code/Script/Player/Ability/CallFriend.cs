using Ivayami.Puzzle;

namespace Ivayami.Player.Ability {
    public class CallFriend : PlayerAbility {

        public override void AbilityStart() {
            if (Friend.Instance.Interacting) {
                Friend.Instance.InteractLongStop();

                Logger.Log(LogType.Player, $"Call Friend - Stop Interacting");
            }
            else if (PlayerActions.Instance.InteractableTarget is IInteractableLong) {
                Friend.Instance.InteractLongWith(PlayerActions.Instance.InteractableTarget as IInteractableLong);

                Logger.Log(LogType.Player, $"Call Friend for '{PlayerActions.Instance.InteractableTarget.gameObject.name}'");
            }
            else Logger.Log(LogType.Player, $"Call Friend - No target");
        }

        public override void AbilityEnd() { }

    }
}
