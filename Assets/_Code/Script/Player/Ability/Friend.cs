using Ivayami.Puzzle;

namespace Ivayami.Player.Ability {
    public class Friend : MonoSingleton<Friend> {

        public IInteractableLong InteractableLongCurrent { get; private set; }

        public void InteractLongWith(IInteractableLong interactableLong) {
            InteractableLongCurrent = interactableLong;
            InteractableLongCurrent.Interact();
        }

        public void InteractLongStop() {
            InteractableLongCurrent?.InteractStop();
            InteractableLongCurrent = null;
        }

    }
}
