using UnityEngine;

namespace Ivayami.Save {
    public class SaveProgressRef : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private AreaProgress _areaProgress;

        public void SetProgressTo(int progress) {
            SaveSystem.Instance.Progress.SaveProgressOfType(_areaProgress.Id, progress);
        }

    }
}
