using System.Linq;
using UnityEditor;
using UnityEngine;
using Ivayami.Save;

namespace Ivayami.UI
{
    [CustomPropertyDrawer(typeof(AreaProgressDataAttribute))]
    public class AreaProgressDataAttributeDrawer : PropertyDrawer
    {
        //private bool _isInitialized;
        private string[] _steps;
        private void Initialize(PlayProfile.AreaProgressData value)
        {
            //_isInitialized = true;
            if (value.AreaProgress) _steps = value.AreaProgress.Steps;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                PlayProfile.AreaProgressData value = new PlayProfile.AreaProgressData(
                (AreaProgress)property.FindPropertyRelative("AreaProgress").objectReferenceValue,
                property.FindPropertyRelative("Step").intValue);

                //if (!_isInitialized)
                Initialize(value);

                DrawStepsDropdown(/*position,*/ property, value);
            }
            else
                DrawStepsWithWarning(position, property, label);
        }

        private void DrawStepsDropdown(/*Rect rect,*/ SerializedProperty property, PlayProfile.AreaProgressData value)
        {
            value.AreaProgress = (AreaProgress)EditorGUILayout.ObjectField(value.AreaProgress, typeof(AreaProgress), false);
            property.FindPropertyRelative("AreaProgress").objectReferenceValue = value.AreaProgress;
            if (_steps != null)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(/*rect,*/ "Step", GUILayout.MaxWidth(40));
                value.Step = EditorGUILayout.Popup(/*rect,*/ value.Step, _steps.ToArray());
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    property.FindPropertyRelative("Step").intValue = value.Step;
                }
            }
        }

        private void DrawStepsWithWarning(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = base.GetPropertyHeight(property, label);
            EditorGUI.HelpBox(position, "Area Progress Data Attribute is only valid for AreaProgressData", MessageType.Warning);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}