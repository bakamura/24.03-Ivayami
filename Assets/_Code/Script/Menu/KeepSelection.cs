using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Ivayami.UI {
    public class KeepSelection : MonoSingleton<KeepSelection> {

        [Header("References")]

        [SerializeField] private InputActionReference[] _clickInputs;

        //[Header("Cache")]

        public GameObject SelectedCurrent { get; private set; }

        protected override void Awake() {
            base.Awake();

            foreach (InputActionReference actionRef in _clickInputs) actionRef.action.performed += PreventSelectNone;
        }

        private void PreventSelectNone(InputAction.CallbackContext context) {
            if (EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(SelectedCurrent);
        }

        public bool CanTriggerSelectEvent(GameObject gameObject) {
            bool can = SelectedCurrent != gameObject;
            SelectedCurrent = gameObject;

            return can;
        }

    }
}
