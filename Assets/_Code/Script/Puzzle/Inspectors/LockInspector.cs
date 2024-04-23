#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(Lock))]
    public class LockInspector : Editor
    {
        SerializedProperty cancelInteractionInput, /*inputActionMap, */
            interactionType,
            itemsRequired, deliverItemsUI, deliverOptionsContainer, navigateUIInput, deliverBtn, onItemDeliverFailed,
            passwordUI,
            onInteract, onCancelInteraction, onActivate;
        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            //EditorGUILayout.PropertyField(inputActionMap, new GUIContent("Player Actiohn Map"));
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            EditorGUILayout.PropertyField(navigateUIInput, new GUIContent("Navigate UI Input"));
            EditorGUILayout.Space(10);

            GUILayout.Label("BEHAVIOUR", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(interactionType, new GUIContent("Interaction Type"));
            switch ((Lock.InteractionTypes)interactionType.enumValueFlag)
            {
                case Lock.InteractionTypes.RequireItems:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(itemsRequired, new GUIContent("Items Required To Unlock"));
                    EditorGUILayout.PropertyField(deliverItemsUI, new GUIContent("Deliver Item UI"));
                    EditorGUILayout.PropertyField(deliverOptionsContainer, new GUIContent("Items Icons Container"));
                    EditorGUILayout.PropertyField(deliverBtn, new GUIContent("Deliver Button"));
                    EditorGUILayout.PropertyField(onItemDeliverFailed, new GUIContent("On Item Deliver Failed"));
                    break;
                case Lock.InteractionTypes.RequirePassword:
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

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            //inputActionMap = serializedObject.FindProperty("_inputActionMap");
            interactionType = serializedObject.FindProperty("_interactionType");
            itemsRequired = serializedObject.FindProperty("_itemsRequired");
            deliverItemsUI = serializedObject.FindProperty("_deliverItemsUI");
            deliverOptionsContainer = serializedObject.FindProperty("_deliverOptionsContainer");
            navigateUIInput = serializedObject.FindProperty("_navigateUIInput");
            deliverBtn = serializedObject.FindProperty("_deliverBtn");
            onItemDeliverFailed = serializedObject.FindProperty("_onItemDeliverFailed");
            passwordUI = serializedObject.FindProperty("_passwordUI");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
            onActivate = serializedObject.FindProperty("onActivate");
        }
    }
}
#endif