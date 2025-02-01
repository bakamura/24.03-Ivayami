using UnityEngine;
using UnityEditor;

namespace Ivayami.Dialogue
{
    [CustomEditor(typeof(DialogueTrigger))]
    public class DialogueTriggerInspector : Editor
    {
        SerializedProperty dialogue, dialogueName, activateOnce, deactivateObjectOnFirstActivate, lockPlayerInput, onDialogueStart, onDialogueEnd;
        private Collider _collider;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(dialogue, new GUIContent("Dialogue"));
            EditorGUILayout.PropertyField(dialogueName, new GUIContent("Dialogue ID"));
            EditorGUILayout.PropertyField(activateOnce, new GUIContent("Activate Once"));
            if (_collider && _collider.isTrigger && activateOnce.boolValue)
                EditorGUILayout.PropertyField(deactivateObjectOnFirstActivate, new GUIContent("Deactivate Object On Activation"));
            EditorGUILayout.PropertyField(lockPlayerInput, new GUIContent("Lock Player Input"));
            EditorGUILayout.PropertyField(onDialogueStart, new GUIContent("On Dialogue Start"));
            EditorGUILayout.PropertyField(onDialogueEnd, new GUIContent("On Dialogue End"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            dialogue = serializedObject.FindProperty("_dialogue");
            dialogueName = serializedObject.FindProperty("_dialogueName");
            activateOnce = serializedObject.FindProperty("_activateOnce");
            deactivateObjectOnFirstActivate = serializedObject.FindProperty("_deactivateObjectOnFirstActivate");
            lockPlayerInput = serializedObject.FindProperty("_lockPlayerInput");
            onDialogueStart = serializedObject.FindProperty("_onDialogueStart");
            onDialogueEnd = serializedObject.FindProperty("_onDialogueEnd");

            DialogueTrigger instance = (DialogueTrigger)target;
            instance.TryGetComponent<Collider>(out _collider);
        }
    }
}