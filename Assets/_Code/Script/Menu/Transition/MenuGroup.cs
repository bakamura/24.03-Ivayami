using System.Collections;
using UnityEngine;

namespace Paranapiacaba.UI {
    public class MenuGroup : MonoBehaviour {

        [SerializeField] private Menu _currentMenu;
        [SerializeField] private float _delayToOpen;
        public bool transitioning { get; private set; }

        public void CloseCurrentThenOpen(Menu menuToOpen) {
            StartCoroutine(CloseCurrentThenOpenRoutine(menuToOpen));
        }

        private IEnumerator CloseCurrentThenOpenRoutine(Menu menuToOpen) {
            Logger.Log(LogType.UI, $"Change Menu Start");
            transitioning = true;
            _currentMenu?.Close();

            yield return new WaitForSeconds(_delayToOpen >= 0 ? _delayToOpen : (_currentMenu != null ? _currentMenu.TransitionDuration : 0f));

            _currentMenu = menuToOpen;
            _currentMenu.Open();
            transitioning = false;
            Logger.Log(LogType.UI, $"Change Menu End");
        }

    }
}