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

            bool isInstant = _delayToOpen < 0;
            bool previousWasNull = _currentMenu == null;
            if (isInstant && !previousWasNull) _currentMenu.OnTransitionEnd.AddListener(Open);

            _currentMenu = menuToOpen;

            if (!isInstant || previousWasNull) {
                if(!isInstant) yield return new WaitForSeconds(_delayToOpen);

                Open();
            }

        }

        private void Open() {
            _currentMenu.Open();
            _transitionCoroutine = null;
            _currentMenu.OnTransitionEnd.RemoveListener(Open);

            Selectable newSelectedObject = _currentMenu.InitialSelected;
            if (newSelectedObject != null) EventSystem.current.SetSelectedGameObject(newSelectedObject.gameObject);

            Logger.Log(LogType.UI, $"Change Menu End");
        }

        public void SetSelected(GameObject selectedObject) { // Move Elsewhere?
            if (selectedObject == null || !selectedObject.GetComponent<Selectable>().interactable) return;
            PreventSelectPointer.Instance.ExecuteIfNotClick(() => EventSystem.current.SetSelectedGameObject(selectedObject));
        }

        public void SetCurrentMenuInitialAsSelected() {
            SetSelected(_currentMenu.InitialSelected?.gameObject);
        }

    }
}