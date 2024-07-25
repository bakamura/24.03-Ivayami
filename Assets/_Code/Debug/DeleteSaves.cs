using UnityEngine;

namespace Ivayami.Save {
    public class DeleteSaves : MonoBehaviour {

        public void DeleteAll() {
            for (byte i = 0; i < 5; i++) SaveSystem.Instance.DeleteProgress(i);
        }

    }
}
