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
            if (_setMenuBtnSelectedOnStart) EventSystem.current.SetSelectedGameObject(_currentMenu.GetComponentInChildren<Button>().gameObject);
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

            yield return new WaitForSeconds(_delayToOpen >= 0 ? _delayToOpen : (_currentMenu != null ? _currentMenu.TransitionDuration : 0f));

            _currentMenu.Open();
            _transitionCoroutine = null;

            GameObject newSelectedObject = _currentMenu.GetComponentInChildren<Button>().gameObject;
            if(newSelectedObject != null) EventSystem.current.SetSelectedGameObject(newSelectedObject);

            Logger.Log(LogType.UI, $"Change Menu End");
        }

    }
}