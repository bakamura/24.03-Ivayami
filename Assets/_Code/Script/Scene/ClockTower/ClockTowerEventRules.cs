using UnityEngine;
using Ivayami.Save;

namespace Ivayami.Scene
{
    [CreateAssetMenu(fileName = "NewClockTowerEventRule", menuName = "Ivayami/ClockTowerEventRule")]
    public class ClockTowerEventRules : ScriptableObject
    {
        [SerializeField, ProgressStep] private ProgressTriggerEvent.ProgressConditionInfo[] _progressConditions;
        public ClockTowerEvent ClockTowerEvent;
        [Tooltip("The time it will take for the event to start")] public Range EventIntervalToStartRange;
        [Min(0f)] public float EventDuration;
        
        [System.Serializable]
        public struct Range
        {
            [Min(0f)] public float Min;
            [Min(0f)] public float Max;
        }

        public bool IsCurrentlyValid()
        {
            for (int i = 0; i < _progressConditions.Length; i++)
            {
                if (!SaveSystem.Instance.Progress.gameProgress.ContainsKey(_progressConditions[i].AreaProgress.Id) ||
                    (SaveSystem.Instance.Progress.gameProgress.ContainsKey(_progressConditions[i].AreaProgress.Id) &&
                    (SaveSystem.Instance.Progress.gameProgress[_progressConditions[i].AreaProgress.Id] < _progressConditions[i].ProgressStepMin ||
                    SaveSystem.Instance.Progress.gameProgress[_progressConditions[i].AreaProgress.Id] > _progressConditions[i].ProgressStepMax)))
                {
                    return false;
                }
            }
            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (EventIntervalToStartRange.Min > EventIntervalToStartRange.Max) EventIntervalToStartRange.Min = EventIntervalToStartRange.Max;
        }
#endif
    }
}