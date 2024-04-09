using UnityEngine;

namespace Paranapiacaba.UI {
    public class Quit : MonoBehaviour {

        public void QuitGame() {
            Logger.Log(LogType.UI, $"Quitting Game");
            Application.Quit();
        }

    }
}