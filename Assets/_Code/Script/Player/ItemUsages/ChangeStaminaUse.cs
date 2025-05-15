using System;
using System.Collections;
using UnityEngine;

namespace Ivayami.Player
{
    [CreateAssetMenu(menuName = "Ivayami/Gameplay/ItemAction/ChangeStaminaUse")]
    public class ChangeStaminaUse : ItemUsageAction
    {
        [field: SerializeField, Tooltip("Multiplicative")] public float StaminaDepletionChange { get; private set; }
        [field: SerializeField, Min(0f)] public float Duration { get; private set; }
        public override IEnumerator ExecuteAtion(Action OnActionSuccess = null, Action OnActionFail = null, Action OnActionEnd = null)
        {
            OnActionSuccess?.Invoke();
            PlayerMovement.Instance.UpdateStaminaDepletionMultiplier(nameof(ChangeStaminaUse), StaminaDepletionChange);
            yield return new WaitForSeconds(Duration);
            PlayerMovement.Instance.UpdateStaminaDepletionMultiplier(nameof(ChangeStaminaUse), StaminaDepletionChange);
            OnActionEnd?.Invoke();
        }
    }
}