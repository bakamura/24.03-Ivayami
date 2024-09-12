#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(TriggerEvent))]
    internal sealed class TriggerEventInspector : Editor
    {
        SerializedProperty triggerType, targetNeedToStayInside, delayToActivateEvent, optionalTag, onExecute;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(triggerType, new GUIContent("Trigger Type"));
            if(triggerType.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(targetNeedToStayInside, new GUIContent("Target Need To Stay Inside"));
            }
            EditorGUILayout.PropertyField(delayToActivateEvent, new GUIContent("Delay To Activate Event"));
            EditorGUILayout.PropertyField(optionalTag, new GUIContent("Optional Tag"));
            EditorGUILayout.PropertyField(onExecute, new GUIContent("On Execute"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            triggerType = serializedObject.FindProperty("_triggerType");
            targetNeedToStayInside = serializedObject.FindProperty("_targetNeedToStayInside");
            delayToActivateEvent = serializedObject.FindProperty("_delayToActivateEvent");
            optionalTag = serializedObject.FindProperty("_optionalTag");
            onExecute = serializedObject.FindProperty("_onExecute");
        }
    }
}
#endif