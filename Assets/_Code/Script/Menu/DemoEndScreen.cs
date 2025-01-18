using UnityEngine;
using Ivayami.Player;

namespace Ivayami.UI {
    public class DemoEndScreen : MonoBehaviour {

        [SerializeField] private ScreenFade _unloadFadeIn;
        private const string PAUSE_KEY = "DemoEndScreen";

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        public void Show() {
            GetComponent<Fade>().Open();
            Pause.Instance.ToggleCanPause(PAUSE_KEY, false);
            PlayerActions.Instance.ChangeInputMap("Menu");
        }

        public void GoToMainMenu() {
            GetComponentInChildren<ScreenFade>().FadeIn();
            Pause.Instance.ToggleCanPause(PAUSE_KEY, true);
        }
        
        public void DestroyThis() {
            Destroy(gameObject);
        }

    }
}
