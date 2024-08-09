using Ivayami.Scene;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Save {
    //[RequireComponent(typeof(BoxCollider))]
    public class ProgressTriggerEvent : MonoBehaviour {

        [Serializable]
        public struct ProgressConditionInfo
        {
            public AreaProgress AreaProgress;
            public int ProgressStepMin;
            public int ProgressStepMax;

            public ProgressConditionInfo(AreaProgress areaProgress, int min, int max)
            {
                AreaProgress = areaProgress;
                ProgressStepMin = min;
                ProgressStepMax = max;
            }
        }
        [SerializeField, ProgressStepAttribute] private ProgressConditionInfo[] _progressConditions;
        [SerializeField] private UnityEvent _onTrigger;

        private void OnTriggerEnter(Collider other) {
            TryTrigger();
        }

        public void TryTrigger()
        {
            for(int i = 0; i < _progressConditions.Length; i++)
            {
                if (SaveSystem.Instance.Progress.progress.ContainsKey(_progressConditions[i].AreaProgress.Id) && 
                    (SaveSystem.Instance.Progress.progress[_progressConditions[i].AreaProgress.Id] < _progressConditions[i].ProgressStepMin ||
                    SaveSystem.Instance.Progress.progress[_progressConditions[i].AreaProgress.Id] > _progressConditions[i].ProgressStepMax))
                {
                    return;
                }
            }
            _onTrigger?.Invoke();
        }

    }
}
