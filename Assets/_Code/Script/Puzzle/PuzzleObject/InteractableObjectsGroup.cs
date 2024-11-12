using Ivayami.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks))]
    public class InteractableObjectsGroup : MonoBehaviour, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private ButtonData[] _options;

        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;

        private InteractableFeedbacks _interatctableFeedbacks;
        private CanvasGroup _canvasGroup
        {
            get
            {
                if (!m_canvasGroup) m_canvasGroup = GetComponentInChildren<CanvasGroup>();
                return m_canvasGroup;
            }
        }
        private CanvasGroup m_canvasGroup;
        private InteractableObjectsGroupButton _currentBtn;
        private bool _setupComplete;

        [Serializable]
        public struct ButtonData
        {
            public GameObject InteractableObject;
            private IInteractable _interactable;
            public IInteractable Interactable
            {
                get
                {
                    if (_interactable == null) _interactable = InteractableObject.GetComponent<IInteractable>();
                    return _interactable;
                }
            }
            public InteractableObjectsGroupButton PuzzleButton;
        }
        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableFeedbacks) _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
                return _interatctableFeedbacks;
            }
        }

        private void Setup()
        {
            if (_setupComplete) return;
            for (int i = 0; i < _options.Length; i++)
            {
                _options[i].PuzzleButton.Setup(_options[i].Interactable, this);
            }
            _setupComplete = true;
        }

        public void ForceInteract() => Interact();

        public PlayerActions.InteractAnimation Interact()
        {
            Setup();
            _onInteract?.Invoke();
            _interatctableFeedbacks.UpdateFeedbacks(false, true);
            UpdateInputs(true);
            UpdateUI(true);
            return PlayerActions.InteractAnimation.Default;
        }

        public void InteractableSelected()
        {
            UpdateInputs(false);
            UpdateUI(false);
            //ReturnAction.Instance.Set(ForceInteract);
        }

        public void SetCurrentSelected(InteractableObjectsGroupButton btn)
        {
            if (_currentBtn) _currentBtn.Interactable.InteratctableFeedbacks.UpdateFeedbacks(false, true);
            _currentBtn = btn;
            if (_currentBtn) _currentBtn.Interactable.InteratctableFeedbacks.UpdateFeedbacks(true, true);
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _cancelInteractionInput.action.started += HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _cancelInteractionInput.action.started -= HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }

        private void UpdateUI(bool isActive)
        {
            _canvasGroup.alpha = isActive ? 1 : 0;
            _canvasGroup.blocksRaycasts = isActive;
            _canvasGroup.interactable = isActive;
            if (isActive)
            {
                if (!_currentBtn) _options[0].PuzzleButton.Button.Select();
                else _currentBtn.Button.Select();
            }
        }

        private void HandleCancelInteraction(InputAction.CallbackContext obj)
        {
            ExitInteraction();
        }

        public void ExitInteraction()
        {
            UpdateInputs(false);
            UpdateUI(false);
            SetCurrentSelected(null);
            _interatctableFeedbacks.UpdateFeedbacks(true, true);
            _onCancelInteraction?.Invoke();
        }

#if UNITY_EDITOR        
        private void OnValidate()
        {
            if (_options == null) return;
            for (int i = 0; i < _options.Length; i++)
            {
                if (_options[i].InteractableObject && !_options[i].InteractableObject.TryGetComponent<IInteractable>(out _))
                    _options[i].InteractableObject = null;
            }
        }

        public void UpdateButtonsArray()
        {
            InteractableObjectsGroupButton[] btns = _canvasGroup.GetComponentsInChildren<InteractableObjectsGroupButton>();
            if (btns == null) return;
            if (_options == null) _options = new ButtonData[btns.Length];
            else Array.Resize(ref _options, btns.Length);
            for (int i = 0; i < _options.Length; i++)
            {
                _options[i].PuzzleButton = btns[i];
            }
        }
#endif
    }
}