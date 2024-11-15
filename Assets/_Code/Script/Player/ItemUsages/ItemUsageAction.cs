using System;
using System.Collections;
using UnityEngine;

namespace Ivayami.Player
{
    public abstract class ItemUsageAction : ScriptableObject
    {
        public abstract IEnumerator ExecuteAtion(Action OnActionEnd = null);
    }
}