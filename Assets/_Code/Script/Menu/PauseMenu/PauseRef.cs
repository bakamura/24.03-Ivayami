using UnityEngine;

namespace Ivayami.UI {
    public class PauseRef : MonoBehaviour {

        [SerializeField] private string _key;

        public void TogglePause(bool canPause) {
            Pause.Instance.ToggleCanPause(_key, canPause);
        }
        
    }
}
