using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class DemoEndScreen : MonoBehaviour {

        [SerializeField] private string _mainMenuBtnName;
        private const string PAUSE_KEY = "DemoEndScreen";

        public void Show() {
            GetComponent<MenuGroup>().CloseCurrentThenOpen(GetComponent<Fade>());
            Pause.Instance.ToggleCanPause(PAUSE_KEY, false);
            PlayerActions.Instance.ChangeInputMap("Menu");
        }

        public void GoToMainMenu() {
            Pause.Instance.ToggleCanPause(PAUSE_KEY, true);

            FindObjectsByType<Button>(FindObjectsSortMode.None).First(btn => btn.name == _mainMenuBtnName).onClick.Invoke();
        }
        
    }
}
