#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(Lock))]
    public class LockInspector : Editor
    {
        SerializedProperty cancelInteractionInput, interactionType, confirmInput, clickInput, unlockDelay,
            passwordUI, deliveryUI,
            onInteract, onCancelInteraction, onActivate, onInteractionFailed;
        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            EditorGUILayout.PropertyField(confirmInput, new GUIContent("Confirm Input"));
            EditorGUILayout.PropertyField(clickInput, new GUIContent("Click Input"));
            EditorGUILayout.Space(10);

            GUILayout.Label("BEHAVIOUR", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(interactionType, new GUIContent("Interaction Type"));
            EditorGUILayout.PropertyField(unlockDelay, new GUIContent("Unlock Delay"));
            switch ((Lock.InteractionTypes)interactionType.enumValueFlag)
            {
                case Lock.InteractionTypes.RequireItems:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(deliveryUI, new GUIContent("Deliver UI"));
                    break;
                case Lock.InteractionTypes.RequirePassword:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(passwordUI, new GUIContent("PasswordUI"));
                    break;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onCancelInteraction, new GUIContent("On Cancel Interaction"));
            EditorGUILayout.PropertyField(onActivate, new GUIContent("On Interaction Complete"));
            EditorGUILayout.PropertyField(onInteractionFailed, new GUIContent("On Interaction Failed"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            interactionType = serializedObject.FindProperty("_interactionType");
            unlockDelay = serializedObject.FindProperty("_unlockDelay");
            confirmInput = serializedObject.FindProperty("_confirmInput");
            clickInput = serializedObject.FindProperty("_clickInput");
            passwordUI = serializedObject.FindProperty("_passwordUI");
            deliveryUI = serializedObject.FindProperty("_deliveryUI");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
            onActivate = serializedObject.FindProperty("onActivate");
            onInteractionFailed = serializedObject.FindProperty("_onInteractionFailed");
        }
    }
}
#endif