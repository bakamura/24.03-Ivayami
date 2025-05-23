using System;
using System.Collections;
using UnityEngine;

namespace Ivayami.Player
{
    [CreateAssetMenu(menuName = "Ivayami/Gameplay/ItemAction/HealOverTime")]
    public class HealOverTime : ItemUsageAction
    {
        [field: SerializeField, Range(0f, 1f)] public float TotalHealAmount { get; private set; }
        [field: SerializeField, Min(0f)] public float HealDuration { get; private set; }

        public override IEnumerator ExecuteAtion(Action OnActionSuccess = null, Action OnActionFail = null, Action OnActionEnd = null)
        {
            if(PlayerStress.Instance.StressCurrent == 0)
            {
                OnActionFail?.Invoke();
                yield break;
            }
            else
            {
                OnActionSuccess?.Invoke();
                float count = 0;
                WaitForFixedUpdate delay = new WaitForFixedUpdate();
                while (count < HealDuration)
                {
                    PlayerStress.Instance.AddStress(PlayerStress.Instance.StressCurrent * -TotalHealAmount * (Time.fixedDeltaTime / HealDuration));
                    count += Time.fixedDeltaTime / HealDuration;
                    yield return delay;
                }
                OnActionEnd?.Invoke();
            }
        }
    }
}
