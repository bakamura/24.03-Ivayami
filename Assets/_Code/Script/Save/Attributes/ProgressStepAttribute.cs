#if UNITY_EDITOR
using UnityEngine;
using System;

namespace Ivayami.Scene
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ProgressStepAttribute : PropertyAttribute
    {

    }
}
#endif