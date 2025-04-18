#if UNITY_EDITOR
using UnityEditor;
#endif
using Ivayami.Player;
using UnityEngine;

namespace Ivayami.UI {
    public class Quit : MonoBehaviour {

        private void Awake()
        {
            PlayerActions.Instance.ResetAbilities(); // Should be elsewhere?
        }

        public void QuitGame() {
            Logger.Log(LogType.UI, $"Quitting Game");
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

    }
}