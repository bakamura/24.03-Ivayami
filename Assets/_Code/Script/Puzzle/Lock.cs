using UnityEngine;
using Ivayami.Player;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Ivayami.Audio;
using System.Collections;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds), typeof(LockPuzzleSounds))]
    public class Lock : Activator, IInteractable
    {
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmInput;

        [SerializeField, Min(0f)] private float _unlockDelay;
        [SerializeField] private InteractionTypes _interactionType;
        [SerializeField] private byte _requestAmountToComplete = 1;
        [SerializeField] private bool _skipDeliverUI;

        [SerializeField] private ItemRequestData[] _itemsRequired;
        [SerializeField] private CanvasGroup _deliverItemsUI;
        [SerializeField, Tooltip("Needs to always contain an odd number off child objects")] private RectTransform _deliverOptionsContainer;
        [SerializeField] private Selectable _deliverBtn;
        //[SerializeField] private UnityEvent _onItemDeliverFailed;

        [SerializeField] private PasswordUI _passwordUI;

        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;
        [SerializeField] private UnityEvent _onInteractionFailed;


        private Image[] _deliverOptions;
        private sbyte _currentRequestIndex = 0;
        private List<ItemRequestData> _currentRequests = new List<ItemRequestData>();
        private InteractableFeedbacks _interatctableFeedbacks;
        private InteractableSounds _interactableSounds;
        private LockPuzzleSounds _lockSounds;
        private WaitForSeconds _unlockWait;
        private Coroutine _unlockCoroutine;
        public InteractableFeedbacks InteratctableHighlight { get => _interatctableFeedbacks; }
        public LockPuzzleSounds LockSounds => _lockSounds;

        [System.Serializable]
        public enum InteractionTypes
        {
            RequireItems,
            RequirePassword
        }

        [System.Serializable]
        private struct ItemRequestData
        {
            public InventoryItem Item;
            public bool UseItem;
            public UnityEvent OnItemDelivered;
        }

        private void Awake()
        {
            _deliverOptions = _deliverOptionsContainer.GetComponentsInChildren<Image>();
            _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _lockSounds = GetComponent<LockPuzzleSounds>();
            _unlockWait = new WaitForSeconds(_unlockDelay);
            if (_itemsRequired != null && _currentRequests.Count == 0)
            {
                for (int i = 0; i < _itemsRequired.Length; i++)
                {
                    _currentRequests.Add(_itemsRequired[i]);
                }
            }
        }

        [ContextMenu("Interact")]
        public PlayerActions.InteractAnimation Interact()
        {
            _onInteract?.Invoke();
            if (_interactionType == InteractionTypes.RequireItems && _skipDeliverUI)
            {
                DeliverItem();
                return PlayerActions.InteractAnimation.Default;
            }
            UpdateInputs(true);
            _interatctableFeedbacks.UpdateFeedbacks(false, true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
            UpdateUIs(true);
            return PlayerActions.InteractAnimation.Default;
        }

        public void TryUnlock()
        {
            if (_unlockCoroutine == null)
            {
                bool isPasswordCorrect = _interactionType == InteractionTypes.RequirePassword && _passwordUI.CheckPassword();
                bool hasDeliveredAllItems = _interactionType == InteractionTypes.RequireItems && _itemsRequired.Length - _currentRequests.Count >= _requestAmountToComplete;
                if (hasDeliveredAllItems || isPasswordCorrect)
                {
                    _unlockCoroutine = StartCoroutine(UnlockFeedbackCoroutine());
                }
                else
                {
                    _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionFailed);
                    _onInteractionFailed?.Invoke();
                }
            }
        }

        private IEnumerator UnlockFeedbackCoroutine()
        {
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionSuccess);
            yield return _unlockWait;
            UpdateUIs(false);
            UpdateInputs(false);
            IsActive = !IsActive;
            _unlockCoroutine = null;
            onActivate?.Invoke();
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _cancelInteractionInput.action.performed += HandleExitInteraction;
                _navigateUIInput.action.performed += HandleNavigateUI;
                if (_interactionType == InteractionTypes.RequirePassword)
                {
                    _passwordUI.OnCheckPassword += TryUnlock;
                    if (_passwordUI is RotateLock) _confirmInput.action.performed += HandleConfirmUI;
                }
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _cancelInteractionInput.action.performed -= HandleExitInteraction;
                _navigateUIInput.action.performed -= HandleNavigateUI;
                if (_interactionType == InteractionTypes.RequirePassword)
                {
                    _passwordUI.OnCheckPassword -= TryUnlock;
                    if (_passwordUI is RotateLock) _confirmInput.action.performed -= HandleConfirmUI;
                }
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }
        private void UpdateUIs(bool isActive)
        {
            if (_interactionType == InteractionTypes.RequirePassword) _passwordUI.UpdateActiveState(isActive);
            else UpdateDeliverItemUI(isActive);
        }

        public void CancelInteraction()
        {
            UpdateUIs(false);
            UpdateInputs(false);
            _interatctableFeedbacks.UpdateFeedbacks(true, true);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.InteractReturn);
            _onCancelInteraction?.Invoke();
        }

        private void HandleExitInteraction(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                CancelInteraction();
            }
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input != Vector2.zero)
            {
                _lockSounds.PlaySound(LockPuzzleSounds.SoundTypes.ChangeOption);
                switch (_interactionType)
                {
                    case InteractionTypes.RequireItems:
                        if (EventSystem.current.currentSelectedGameObject == null)
                            _deliverBtn.Select();
                        else if (input.x != 0)
                        {
                            int temp = input.x > 0 ? 1 : -1;
                            _currentRequestIndex += (sbyte)temp;
                            LoopValueByArraySize(ref _currentRequestIndex, _currentRequests.Count);
                            UpdateDeliverIcons((byte)_currentRequestIndex);
                        }
                        break;
                }
            }
        }

        private void HandleConfirmUI(InputAction.CallbackContext context)
        {
            TryUnlock();
        }

        #region DeliverUI

        private void UpdateDeliverItemUI(bool isActive)
        {
            _deliverItemsUI.alpha = isActive ? 1 : 0;
            _deliverItemsUI.interactable = isActive;
            _deliverItemsUI.blocksRaycasts = isActive;

            if (isActive)
            {
                _currentRequestIndex = 0;                
                UpdateDeliverIcons(0);
                _deliverBtn.Select();
            }
        }

        private void UpdateDeliverIcons(byte startIndex)
        {
            for (int i = 0; i < _deliverOptions.Length; i++)
            {
                _deliverOptions[i].enabled = false;
            }
            byte iconsIndex = (byte)Mathf.FloorToInt(_deliverOptions.Length / 2);
            byte iconsFilled = 0;
            byte requestIndex = startIndex;
            while (iconsFilled < _currentRequests.Count && iconsFilled < _deliverOptions.Length)
            {
                _deliverOptions[iconsIndex].enabled = true;
                _deliverOptions[iconsIndex].sprite = _currentRequests[requestIndex].Item.Sprite;
                iconsIndex++;
                requestIndex++;
                iconsFilled++;
                if (iconsIndex == _deliverOptions.Length) iconsIndex = 0;
                if (requestIndex == _currentRequests.Count) requestIndex = 0;
            }
        }

        //called by interface Btn
        public void DeliverItem()
        {
            if (_skipDeliverUI)
            {
                for (int i = 0; i < _currentRequests.Count; i++)
                {
                    if (PlayerInventory.Instance.CheckInventoryFor(_currentRequests[i].Item.name))
                    {
                        RemoveItemFromRequestList(i);
                    }
                }
            }
            else
            {
                _lockSounds.PlaySound(LockPuzzleSounds.SoundTypes.ConfirmOption);
                if (PlayerInventory.Instance.CheckInventoryFor(_currentRequests[_currentRequestIndex].Item.name))
                {
                    RemoveItemFromRequestList(_currentRequestIndex);
                    ConstrainValueToArraySize(ref _currentRequestIndex, _currentRequests.Count);
                    if (_currentRequests.Count > 0) UpdateDeliverIcons((byte)_currentRequestIndex);
                    //TryUnlock();
                    //return;
                }
                //_onItemDeliverFailed?.Invoke();
            }
            TryUnlock();
        }

        private void RemoveItemFromRequestList(int index)
        {
            _currentRequests[index].OnItemDelivered?.Invoke();
            if (_currentRequests[index].UseItem) PlayerInventory.Instance.RemoveFromInventory(_currentRequests[index].Item);
            _currentRequests.RemoveAt(index);
        }

        private void LoopValueByArraySize(ref sbyte valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = (sbyte)(arraySize - 1);
            else if (valueToConstrain >= arraySize) valueToConstrain = 0;
        }

        private void ConstrainValueToArraySize(ref sbyte valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = 0;
            else if (valueToConstrain >= arraySize) valueToConstrain = (sbyte)(arraySize - 1);
        }
        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_requestAmountToComplete > _itemsRequired.Length) _requestAmountToComplete = (byte)_itemsRequired.Length;
        }
#endif
    }
}