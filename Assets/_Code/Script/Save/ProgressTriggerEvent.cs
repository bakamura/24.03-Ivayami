using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Save {
    [RequireComponent(typeof(BoxCollider))]
    public class ProgressTriggerEvent : MonoBehaviour {

        [Serializable]
        private struct ProgressConditionInfo
        {
            public AreaProgress AreaProgress;
            public string ProgressStepMin;
            public string ProgressStepMax;
        }

        [SerializeField] private UnityEvent _onTrigger = new UnityEvent();
        [SerializeField] private string _progressType;
        [SerializeField] private int _progressAmountMin;
        [SerializeField] private int _progressAmountMax;

        private void OnTriggerEnter(Collider other) {
            if (_progressAmountMin <= SaveSystem.Instance.Progress.progress[_progressType] && SaveSystem.Instance.Progress.progress[_progressType] <= _progressAmountMax) _onTrigger.Invoke();
        }

    }
}
