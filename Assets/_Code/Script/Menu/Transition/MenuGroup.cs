using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class MenuGroup : MonoBehaviour {

        [SerializeField] private Menu _currentMenu;
        [SerializeField] private float _delayToOpen;
        private Coroutine _transitionCoroutine;
        [SerializeField] private bool _setMenuBtnSelectedOnStart;
        
        private void Start() {
            if (_setMenuBtnSelectedOnStart) EventSystem.current.SetSelectedGameObject(_currentMenu.InitialSelected.gameObject);
        }

        public void CloseCurrent() {
            if (_currentMenu != null) {
                _currentMenu.Close();
                _currentMenu = null;
            }
        }

        public void CloseCurrentThenOpen(Menu menuToOpen) {
            if (menuToOpen != _currentMenu) {
                if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = StartCoroutine(CloseCurrentThenOpenRoutine(menuToOpen));
            }
        }

        private IEnumerator CloseCurrentThenOpenRoutine(Menu menuToOpen) {
            Logger.Log(LogType.UI, $"Change Menu Start");

            _currentMenu?.Close();
            _currentMenu = menuToOpen;

            Selectable newSelectedObject = _currentMenu.GetComponentInChildren<Selectable>();
            if (newSelectedObject != null) EventSystem.current.SetSelectedGameObject(newSelectedObject.gameObject);

            if (_delayToOpen >= 0) {
                yield return new WaitForSeconds(_delayToOpen);

                Open();
            }
            else if (_currentMenu != null) _currentMenu.OnTransitionEnd.AddListener(_currentMenu.Open);
        }

        private void Open() {
            _currentMenu.Open();
            _transitionCoroutine = null;
            _currentMenu.OnTransitionEnd.RemoveListener(_currentMenu.Open);

            Logger.Log(LogType.UI, $"Change Menu End");
        }

        public void SetSelected(GameObject selectedObject) { // Move Elsewhere?
            PreventSelectPointer.Instance.ExecuteIfNotClick(() => EventSystem.current.SetSelectedGameObject(selectedObject));
        }

        public void SetCurrentMenuInitialAsSelected() {
            SetSelected(_currentMenu.InitialSelected?.gameObject);
        }

    }
}