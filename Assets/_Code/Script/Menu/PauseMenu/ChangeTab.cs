using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class ChangeTab : MonoBehaviour {

        [Header("References")]

        [SerializeField] private InputActionReference _tabChangeAction;
        [SerializeField] private Button[] _tabBtns;
        [SerializeField] private Highlightable[] _tabHighlightables;
        [SerializeField] private HighlightGroup _highlightGroup;

        private int _tabCurrent;

        private void Start() {
            Pause.Instance.onPause.AddListener(() => _tabChangeAction.action.performed += Change);
            Pause.Instance.onUnpause.AddListener(() => _tabChangeAction.action.performed -= Change);
        }

        private void Change(InputAction.CallbackContext context) {
            _tabCurrent += (int)context.ReadValue<float>();
            if (_tabCurrent < 0) _tabCurrent += _tabBtns.Length;
            else if (_tabCurrent >= _tabBtns.Length) _tabCurrent -= _tabBtns.Length;

            _tabBtns[_tabCurrent].Select();
            _tabBtns[_tabCurrent].onClick.Invoke();
        }

        public void HighlightCurrentTab() {
            _highlightGroup.SetHighlightTo(_tabHighlightables[_tabCurrent]);
        }

    }
}
