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

        public int tabCurrent { private get; set; }

        private void Start() {
            Pause.Instance.onPause.AddListener(() => _tabChangeAction.action.performed += Change);
            Pause.Instance.onUnpause.AddListener(() => _tabChangeAction.action.performed -= Change);
            HighlightCurrentTab();
        }

        private void Change(InputAction.CallbackContext context) {
            tabCurrent += (int)context.ReadValue<float>();
            if (tabCurrent < 0) tabCurrent += _tabBtns.Length;
            else if (tabCurrent >= _tabBtns.Length) tabCurrent -= _tabBtns.Length;
            ChangeTo(tabCurrent);
        }

        public void ChangeTo(int tab) {
            tabCurrent = tab;
            _tabBtns[tabCurrent].Select();
            _tabBtns[tabCurrent].onClick.Invoke();
        }

        public void HighlightCurrentTab() {
            _highlightGroup.SetHighlightTo(_tabHighlightables[tabCurrent]);
        }

    }
}
