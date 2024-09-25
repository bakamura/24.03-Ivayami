
namespace Ivayami.UI {
    public class Fade : Menu {

        protected override void TransitionBehaviour(float currentPhase) {
            _canvasGroup.alpha = _transitionCurve.Evaluate(_isOpening ? currentPhase : (1f - currentPhase));
        }

    }
}