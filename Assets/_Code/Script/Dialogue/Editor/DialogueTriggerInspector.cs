using UnityEngine;
using UnityEditor;

namespace Ivayami.Dialogue
{
    [CustomEditor(typeof(DialogueTrigger))]
    public class DialogueTriggerInspector : Editor
    {
        SerializedProperty dialogue, dialogueName, activateOnce, deactivateObjectOnFirstActivate, lockPlayerInput;
        private bool hasCollider;
        public override void OnInspectorGUI()
        {            
            EditorGUILayout.PropertyField(dialogue, new GUIContent("Dialogue"));
            EditorGUILayout.PropertyField(dialogueName, new GUIContent("Dialogue ID"));
            EditorGUILayout.PropertyField(activateOnce, new GUIContent("Activate Once"));
            if (hasCollider && activateOnce.boolValue) 
                EditorGUILayout.PropertyField(deactivateObjectOnFirstActivate, new GUIContent("Deactivate Object On Activation"));
            EditorGUILayout.PropertyField(lockPlayerInput, new GUIContent("Lock Player Input"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            dialogue = serializedObject.FindProperty("_dialogue");
            dialogueName = serializedObject.FindProperty("_dialogueName");
            activateOnce = serializedObject.FindProperty("_activateOnce");
            deactivateObjectOnFirstActivate = serializedObject.FindProperty("_deactivateObjectOnFirstActivate");
            lockPlayerInput = serializedObject.FindProperty("_lockPlayerInput");

            DialogueTrigger instance = (DialogueTrigger)target;
            hasCollider = instance.GetComponent<Collider>();
        }
    }
}