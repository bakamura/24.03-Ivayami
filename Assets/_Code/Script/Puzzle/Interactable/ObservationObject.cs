using Ivayami.Audio;
using Ivayami.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.UI;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableSounds))]
    public class ObservationObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private InputActionReference _exitInput;
        [SerializeField] private InputActionReference _toggleImageContentInput;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onExitInteraction;

        private Fade _displayContentFade
        {
            get
            {
                if (!m_displayContentFade)
                {
                    m_displayContentFade = GetComponentInChildren<Fade>();
                    _contentImage = m_displayContentFade.GetComponentInChildren<Image>();
                }
                return m_displayContentFade;
            }
        }
        private Fade m_displayContentFade;
        private Image _contentImage;
        private bool _isContentOpen;
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
                _exitInput.action.started += HandleExitInput;
                if (_toggleImageContentInput) _toggleImageContentInput.action.started += HandleToggleContentInput;
                PlayerMovement.Instance.ToggleMovement(nameof(ObservationObject), false);
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _exitInput.action.started -= HandleExitInput;
                if (_toggleImageContentInput) _toggleImageContentInput.action.started -= HandleToggleContentInput;
                PlayerMovement.Instance.ToggleMovement(nameof(ObservationObject), true);
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }

        private void UpdateDisplayImage(bool isActive)
        {
            if (isActive) _displayContentFade.Open();
            else _displayContentFade.Close();
            _isContentOpen = isActive;
        }

        private void HandleExitInput(InputAction.CallbackContext context)
        {
            UpdateInputs(false);
            if (_isContentOpen) UpdateDisplayImage(false);
            InteratctableFeedbacks.UpdateFeedbacks(true, true);
            _onExitInteraction?.Invoke();
        }

        private void HandleToggleContentInput(InputAction.CallbackContext context)
        {
            UpdateDisplayImage(!_isContentOpen);
        }

    }
}