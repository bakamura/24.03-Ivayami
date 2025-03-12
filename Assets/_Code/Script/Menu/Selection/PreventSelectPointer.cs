using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Ivayami.Player;

namespace Ivayami.UI {
    public class PreventSelectPointer : MonoSingleton<PreventSelectPointer> {

        [Header("References")]

        [SerializeField] private InputActionReference _pointerClickAction;

        [Header("Cache")]

        private bool _isClicking = false;
        private Action _onReleasePointer;

        protected override void Awake() {
            base.Awake();

            _pointerClickAction.action.performed += ClickUpdate;
        }

        private void ClickUpdate(InputAction.CallbackContext context) {
            _isClicking = context.ReadValue<float>() == 1;
            if (!_isClicking) {
                _onReleasePointer?.Invoke();
                _onReleasePointer = null;
            }
        }

        public void ExecuteIfNotClick(Action action) {
            if (InputCallbacks.Instance.IsGamepad || !_isClicking) action.Invoke();
            else _onReleasePointer = action;
        }

    }
}
