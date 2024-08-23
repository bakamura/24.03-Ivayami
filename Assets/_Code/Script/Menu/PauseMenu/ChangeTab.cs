using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class ChangeTab : MonoBehaviour {

        [Header("References")]

        [SerializeField] private InputActionReference _tabChangeAction;
        [SerializeField] private Button[] _tabBtns;

        private int _tabCurrent;

        private void Start() {
            Pause.Instance.onPause.AddListener(() => _tabChangeAction.action.performed += Change);
            Pause.Instance.onUnpause.AddListener(() => _tabChangeAction.action.performed -= Change);
        }

        private void Change(InputAction.CallbackContext context) {
            Debug.Log(_tabCurrent);
            Debug.Log(context.ReadValue<int>());
            _tabCurrent += context.ReadValue<int>();
            if (_tabCurrent < 0) _tabCurrent += _tabBtns.Length;
            else if (_tabCurrent >= _tabBtns.Length) _tabCurrent -= _tabBtns.Length;
            Debug.Log(_tabCurrent);

            _tabBtns[_tabCurrent].Select();
            _tabBtns[_tabCurrent].onClick.Invoke();
        }

    }
}
