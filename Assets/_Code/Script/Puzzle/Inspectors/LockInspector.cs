#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paranapiacaba.Puzzle
{
    [CustomEditor(typeof(Lock))]
    public class LockInspector : Editor
    {
        SerializedProperty cancelInteractionInput, inputActionMap, interactionType, itemsRequired, deliverItemsUI, passwordRequired, passwordTextField, passwordUI, onInteract, onCancelInteraction, onActivate;
        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(inputActionMap, new GUIContent("Player Actiohn Map"));
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            EditorGUILayout.Space(10);

            GUILayout.Label("BEHAVIOUR", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(interactionType, new GUIContent("Interaction Type"));
            switch ((Lock.InteractionTypes)interactionType.enumValueFlag)
            {
                case Lock.InteractionTypes.RequireItems:
                    EditorGUILayout.PropertyField(itemsRequired, new GUIContent("Items Required To Unlock"));
                    EditorGUILayout.PropertyField(deliverItemsUI, new GUIContent("Deliver Item UI"));
                    break;
                case Lock.InteractionTypes.RequirePassword:
                    EditorGUILayout.PropertyField(passwordRequired, new GUIContent("Password"));
                    EditorGUILayout.PropertyField(passwordUI, new GUIContent("PasswordUI"));
                    EditorGUILayout.PropertyField(passwordTextField, new GUIContent("Password Display"));
                    break;
            }
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onCancelInteraction, new GUIContent("On Cancel Interaction"));
            EditorGUILayout.PropertyField(onActivate, new GUIContent("On Activate"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            inputActionMap = serializedObject.FindProperty("_inputActionMap");
            interactionType = serializedObject.FindProperty("_interactionType");
            itemsRequired = serializedObject.FindProperty("_itemsRequired");
            deliverItemsUI = serializedObject.FindProperty("_deliverItemsUI");
            passwordRequired = serializedObject.FindProperty("_passwordRequired");
            passwordTextField = serializedObject.FindProperty("_passwordTextField");
            passwordUI = serializedObject.FindProperty("_passwordUI");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
            onActivate = serializedObject.FindProperty("onActivate");
        }
    }
}
#endif