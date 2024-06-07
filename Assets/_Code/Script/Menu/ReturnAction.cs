using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ivayami.UI {
    public class ReturnAction : MonoSingleton<ReturnAction> {

        [SerializeField] private InputActionReference _returnInput;
        private Action _action;

        protected override void Awake() {
            base.Awake();

            _returnInput.action.performed += Do;
        }

        public void Set(Action action) {
            _action = action;
        }

        public void Do(InputAction.CallbackContext context) {
            if (_action != null) {
                _action.Invoke();
                _action = null;
            }
        }

    }
}
