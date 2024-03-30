using UnityEngine;
using UnityEngine.Events;

namespace Paranapiacaba.UI {
    public class Pause : MonoSingleton<Pause> {

        public UnityEvent<bool> onPause = new UnityEvent<bool>();

        public void PauseGame(bool isPausing) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}