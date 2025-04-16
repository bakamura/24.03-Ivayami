using UnityEngine;

namespace Ivayami.Save {
    public class SaveProgressRef : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private AreaProgress _areaProgress;

        public virtual void SetProgressTo(int progress) {
            SaveSystem.Instance.Progress.SaveProgressOfType(_areaProgress.Id, progress);
        }

        public virtual void SetEntryProgressTo(int progress) {
            SaveSystem.Instance.Progress.SaveEntryProgressOfType(_areaProgress.Id, progress);
        }

    }
}
