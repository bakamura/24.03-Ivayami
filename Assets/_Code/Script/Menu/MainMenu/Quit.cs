using UnityEngine;

namespace Ivayami.UI {
    public class Quit : MonoBehaviour {

        private void Awake() {
            Pause.Instance.canPause = false;
        }

        public void QuitGame() {
            Logger.Log(LogType.UI, $"Quitting Game");
            Application.Quit();
        }

    }
}