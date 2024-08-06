#if UNITY_EDITOR
using Ivayami.Scene;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ivayami.Save
{
    [CustomPropertyDrawer(typeof(ProgressStepAttribute))]
    public class ProgressStepAttributeDrawer : PropertyDrawer
    {

    }
}
#endif