#if UNITY_EDITOR
using Ivayami.Scene;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ivayami.Save
{
    [CustomPropertyDrawer(typeof(ProgressStepAttribute))]
    public class ProgressStepAttributeDrawer : PropertyDrawer
    {
        //private bool _isInitialized;
        private string[] _steps;
        private void Initialize(ProgressTriggerEvent.ProgressConditionInfo value)
        {
            //_isInitialized = true;
            if (value.AreaProgress) _steps = value.AreaProgress.Steps;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                ProgressTriggerEvent.ProgressConditionInfo value = new ProgressTriggerEvent.ProgressConditionInfo(
                (AreaProgress)property.FindPropertyRelative("AreaProgress").objectReferenceValue,
                property.FindPropertyRelative("ProgressStepMin").intValue,
                property.FindPropertyRelative("ProgressStepMax").intValue);

                //if (!_isInitialized)
                Initialize(value);

                DrawStepsDropdown(/*position,*/ property, value, label);
            }
            else
                DrawStepsWithWarning(position, property, label);
        }

        private void DrawStepsDropdown(/*Rect position, */SerializedProperty property, ProgressTriggerEvent.ProgressConditionInfo value, GUIContent label)
        {
            value.AreaProgress = (AreaProgress)EditorGUILayout.ObjectField(value.AreaProgress, typeof(AreaProgress), false);
            property.FindPropertyRelative("AreaProgress").objectReferenceValue = value.AreaProgress;
            if(_steps != null)
            {
                EditorGUI.BeginChangeCheck();
                value.ProgressStepMin = EditorGUILayout.Popup(value.ProgressStepMin, _steps.ToArray());
                value.ProgressStepMax = EditorGUILayout.Popup(value.ProgressStepMax, _steps.ToArray());
                if (EditorGUI.EndChangeCheck())
                {                
                    property.FindPropertyRelative("ProgressStepMin").intValue = value.ProgressStepMin;
                    property.FindPropertyRelative("ProgressStepMax").intValue = value.ProgressStepMax;
                }
            }
        }

        private void DrawStepsWithWarning(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = base.GetPropertyHeight(property, label);
            EditorGUI.HelpBox(position, "Progress Step Attribute is only valid for ProgressConditionInfo", MessageType.Warning);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif