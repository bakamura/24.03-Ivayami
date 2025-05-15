using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Player
{
    public class ChangeStaminaUse : ItemUsageAction
    {
        [field: SerializeField, Range(0f, 1f)] public float TotalHealAmount { get; private set; }
        [field: SerializeField, Min(0f)] public float HealDuration { get; private set; }
        public override IEnumerator ExecuteAtion(Action OnActionSuccess = null, Action OnActionFail = null, Action OnActionEnd = null)
        {
            throw new NotImplementedException();
        }
    }
}