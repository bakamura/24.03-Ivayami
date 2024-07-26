using UnityEngine;

namespace Ivayami.UI {
    public class InfoUpdateIndicatorRef : MonoBehaviour {

        public void DisplayMapUpdate() {
            InfoUpdateIndicator.Instance.DisplayMap();
        }

        public void DisplayReadableAcquired() {
            InfoUpdateIndicator.Instance.DisplayReadable();
        }

    }
}
