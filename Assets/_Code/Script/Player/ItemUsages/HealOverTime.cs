using System;
using System.Collections;
using UnityEngine;

namespace Ivayami.Player
{
    [CreateAssetMenu(menuName = "Inventory/ItemAction/HealOverTime")]
    public class HealOverTime : ItemUsageAction
    {
        [field: SerializeField, Range(0f, 1f)] public float TotalHealAmount { get; private set; }
        [field: SerializeField, Min(0f)] public float HealDuration { get; private set; }

        public override IEnumerator ExecuteAtion(Action OnActionEnd = null) => HealOverTimeCoroutine(OnActionEnd);
        private IEnumerator HealOverTimeCoroutine(Action OnActionEnd = null)
        {
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
