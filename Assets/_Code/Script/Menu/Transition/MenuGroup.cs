using System.Collections;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class MenuGroup : MonoBehaviour {

        [SerializeField] private Menu _currentMenu;
        [SerializeField] private float _delayToOpen;
        private Coroutine _transitionCoroutine;



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
            Logger.Log(LogType.UI, $"Change Menu End");
        }

    }
}