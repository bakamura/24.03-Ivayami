
namespace Ivayami.UI {
    public class Fade : Menu {

        protected override void TransitionBehaviour(float currentPhase) {
            _canvasGroup.alpha = _isOpening ? currentPhase : (1f - currentPhase);
        }

    }
}