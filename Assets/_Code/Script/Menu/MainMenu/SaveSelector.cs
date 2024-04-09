using UnityEngine;

namespace Paranapiacaba.UI {
    public class SaveSelector : MonoBehaviour {

        private byte _displayedSaveId;

        public void DisplaySaveInfo(byte saveId) {
            _displayedSaveId = saveId;
            Logger.Log(LogType.UI, $"Display Save {saveId}");
        }

        public void EnterSave() {
            Logger.Log(LogType.UI, $"Display Save {_displayedSaveId}");
        }

    }
}