using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Ivayami.UI {
    public class KeepSelection : MonoBehaviour {

        [Header("References")]

        [SerializeField] private InputActionReference[] _clickInputs;

        [Header("Cache")]

        private EventSystem _eventSystem;
        private GameObject _objectSelected;

        private void Awake() {
            _eventSystem = GetComponent<EventSystem>();
            foreach(InputActionReference actionRef in _clickInputs) actionRef.action.performed += PreventSelectNone;
        }

        private void PreventSelectNone(InputAction.CallbackContext context) {
            if (_eventSystem.currentSelectedGameObject != _objectSelected) {
                if (_eventSystem.currentSelectedGameObject != null) _objectSelected = _eventSystem.currentSelectedGameObject;
                else _eventSystem.SetSelectedGameObject(_objectSelected);
            }
        }

    }
}
