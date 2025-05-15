using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Player
{
    public class RunSpeedChange : ItemUsageAction
    {
        [field: SerializeField] public float SpeedChange { get; private set; }
        [field: SerializeField, Min(0f)] public float Duration { get; private set; }
        public override IEnumerator ExecuteAtion(Action OnActionSuccess = null, Action OnActionFail = null, Action OnActionEnd = null)
        {
            OnActionSuccess?.Invoke();
            PlayerMovement.Instance.UpdateRunSpeedMultiplier(nameof(RunSpeedChange), SpeedChange);
            yield return new WaitForSeconds(Duration);
            PlayerMovement.Instance.UpdateRunSpeedMultiplier(nameof(RunSpeedChange), SpeedChange);
            OnActionEnd?.Invoke();
        }
    }
}