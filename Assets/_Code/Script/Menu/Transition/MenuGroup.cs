using System.Collections;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class MenuGroup : MonoBehaviour {

        [SerializeField] private Menu _currentMenu;
        [SerializeField] private float _delayToOpen;

        public void CloseCurrentThenOpen(Menu menuToOpen) {
            StartCoroutine(CloseCurrentThenOpenRoutine(menuToOpen));
        }

        private IEnumerator CloseCurrentThenOpenRoutine(Menu menuToOpen) {
            _currentMenu?.Close();

            yield return new WaitForSeconds(_delayToOpen >= 0 ? _delayToOpen : (_currentMenu != null ? _currentMenu.TransitionDuration : 0f));

            _currentMenu = menuToOpen;
            _currentMenu.Open();
        }

    }
}