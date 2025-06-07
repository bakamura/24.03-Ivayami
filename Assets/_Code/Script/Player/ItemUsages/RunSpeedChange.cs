using System;
using System.Collections;
using UnityEngine;

namespace Ivayami.Player
{
    [CreateAssetMenu(menuName = "Ivayami/Gameplay/ItemAction/RunSpeedChange")]
    public class RunSpeedChange : ItemUsageAction
    {
        [field: SerializeField, Tooltip("Multiplicative")] public float SpeedChange { get; private set; }
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