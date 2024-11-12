using Ivayami.Audio;
using Ivayami.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableSounds))]
    public class ObservationObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private InputActionReference _exitInput;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onExitInteraction;

        private InteractableFeedbacks m_interatctableHighlight;
        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }
        }
        private InteractableSounds m_interactableSounds;

        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!m_interatctableHighlight && this) m_interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return m_interatctableHighlight;
            }
        }

        public PlayerActions.InteractAnimation Interact()
        {
            UpdateInputs(true);
            InteratctableFeedbacks.UpdateFeedbacks(false, true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            _onInteract?.Invoke();
            return PlayerActions.InteractAnimation.Default;
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _exitInput.action.performed += HandleExitInput;
                PlayerMovement.Instance.ToggleMovement(nameof(ObservationObject), false);
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _exitInput.action.performed -= HandleExitInput;
                PlayerMovement.Instance.ToggleMovement(nameof(ObservationObject), true);
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }

        private void HandleExitInput(InputAction.CallbackContext context)
        {
            UpdateInputs(false);
            InteratctableFeedbacks.UpdateFeedbacks(true, true);
            _onExitInteraction?.Invoke();
        }
    }
}