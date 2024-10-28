#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Ivayami.UI {
    public class Quit : MonoBehaviour {

        public void QuitGame() {
            Logger.Log(LogType.UI, $"Quitting Game");
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

    }
}