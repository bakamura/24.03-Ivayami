using UnityEngine;

namespace Ivayami.Save {
    public class DeleteSavesRef : MonoBehaviour {

        public void DeleteAll() {
            for (byte i = 0; i < 5; i++) SaveSystem.Instance.DeleteProgress(i);
        }

    }
}
