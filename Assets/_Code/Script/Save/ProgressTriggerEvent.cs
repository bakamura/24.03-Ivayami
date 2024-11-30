using Ivayami.Scene;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Save {
    public class ProgressTriggerEvent : MonoBehaviour {

        [SerializeField] private bool _triggerOnStart;
        [Serializable]
        public struct ProgressConditionInfo {
            public AreaProgress AreaProgress;
            public int ProgressStepMin;
            public int ProgressStepMax;

            public ProgressConditionInfo(AreaProgress areaProgress, int min, int max) {
                AreaProgress = areaProgress;
                ProgressStepMin = min;
                ProgressStepMax = max;
            }
        }
        [SerializeField, ProgressStepAttribute] private ProgressConditionInfo[] _progressConditions;
        [SerializeField] private UnityEvent _onTrigger;

        private void Start() {
            if (_triggerOnStart) TryTrigger();
        }

        private void OnTriggerEnter(Collider other) {
            TryTrigger();
        }

        public void TryTrigger() {
            if (!SaveSystem.Instance || SaveSystem.Instance.Progress == null) return;
            for (int i = 0; i < _progressConditions.Length; i++) {
                if (!SaveSystem.Instance.Progress.gameProgress.ContainsKey(_progressConditions[i].AreaProgress.Id)) continue;
                else if (SaveSystem.Instance.Progress.gameProgress[_progressConditions[i].AreaProgress.Id] < _progressConditions[i].ProgressStepMin ||
                         SaveSystem.Instance.Progress.gameProgress[_progressConditions[i].AreaProgress.Id] > _progressConditions[i].ProgressStepMax) continue;
                _onTrigger?.Invoke();
            }
        }
    }

}

