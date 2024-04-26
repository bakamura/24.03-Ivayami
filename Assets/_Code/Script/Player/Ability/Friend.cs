using Ivayami.Puzzle;

namespace Ivayami.Player.Ability {
    public class Friend : MonoSingleton<Friend> {

        private IInteractableLong _interactableLongCache;

        public bool Interacting { get { return _interactableLongCache != null; } }

        public void InteractLongWith(IInteractableLong interactableLong) {
            _interactableLongCache = interactableLong;
            _interactableLongCache.Interact();
        }

        public void InteractLongStop() {
            _interactableLongCache?.InteractStop();
            _interactableLongCache = null;
        }

    }
}
