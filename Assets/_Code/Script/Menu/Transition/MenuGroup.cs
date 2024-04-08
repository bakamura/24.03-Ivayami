using System.Collections;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class MenuGroup : MonoBehaviour {

        [SerializeField] private Menu _currentMenu;
        [SerializeField] private float _delayToOpen;

        public MenuGroup(Menu currentMenu,float delayToOpen) {
            _currentMenu = currentMenu;
            _delayToOpen = delayToOpen;
        }

        public void CloseCurrentThenOpen(Menu menuToOpen) {
            StartCoroutine(CloseCurrentThenOpenRoutine(menuToOpen));
        }

        private IEnumerator CloseCurrentThenOpenRoutine(Menu menuToOpen) {
            _currentMenu?.Close();

            yield return new WaitForSeconds(_delayToOpen >= 0 ? _delayToOpen : _currentMenu.TransitionDuration);

            _currentMenu = menuToOpen;
            _currentMenu.Open();
        }

    }
}