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

        private InteractableFeedbacks _interatctableHighlight;
        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }
        }
        private InteractableSounds m_interactableSounds;

        public InteractableFeedbacks InteratctableHighlight
        {
            get
            {
                if (!_interatctableHighlight) _interatctableHighlight = GetComponent<InteractableFeedbacks>();
                return _interatctableHighlight;
            }
        }

        public void Interact()
        {
            UpdateInputs(true);
            InteratctableHighlight.UpdateFeedbacks(false);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            _onInteract?.Invoke();
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _exitInput.action.performed += HandleExitInput;
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _exitInput.action.performed -= HandleExitInput;
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }

        private void HandleExitInput(InputAction.CallbackContext context)
        {
            UpdateInputs(false);
            InteratctableHighlight.UpdateFeedbacks(true);
            _onExitInteraction?.Invoke();
        }
    }
}