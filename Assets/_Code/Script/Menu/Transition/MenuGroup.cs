using UnityEngine;

namespace Paranapiacaba.UI {
    public class MenuGroup : MonoBehaviour {

        [SerializeField] private Menu _currentMenu;
        [SerializeField] private float _delayToOpen;

        public void CloseCurrentThenOpen(Menu menuToOpen) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}