using UnityEngine;

namespace Ivayami.UI {
    public class WebHelper : MonoBehaviour {

        public void OpenUrl(string url) {
            Application.OpenURL(url);
        }
        
    }
}
