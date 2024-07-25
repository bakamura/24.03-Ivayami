using UnityEngine;

namespace Ivayami.Save {
    public class SaveProgressRef : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField] private string _saveName;

        public void SetProgressTo(int progress) {
            SaveSystem.Instance.Progress.SaveProgressOfType(_saveName, progress);
        }

    }
}
