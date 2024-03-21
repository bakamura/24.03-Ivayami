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
        SerializedProperty cancelInteractionInput, inputActionMap, 
            interactionType, 
            itemsRequired, deliverItemsUI, deliverOptionsContainer, navigateUIInput, deliverBtn, 
            passwordUI, 
            onInteract, onCancelInteraction, onActivate;
        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(inputActionMap, new GUIContent("Player Actiohn Map"));
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            if((Lock.InteractionTypes)interactionType.enumValueFlag == Lock.InteractionTypes.RequireItems) 
                EditorGUILayout.PropertyField(navigateUIInput, new GUIContent("Navigate UI Input"));
            EditorGUILayout.Space(10);

            GUILayout.Label("BEHAVIOUR", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(interactionType, new GUIContent("Interaction Type"));
            switch ((Lock.InteractionTypes)interactionType.enumValueFlag)
            {
                case Lock.InteractionTypes.RequireItems:
                    EditorGUILayout.PropertyField(itemsRequired, new GUIContent("Items Required To Unlock"));
                    EditorGUILayout.PropertyField(deliverItemsUI, new GUIContent("Deliver Item UI"));
                    EditorGUILayout.PropertyField(deliverOptionsContainer, new GUIContent("Items Icons Container"));
                    EditorGUILayout.PropertyField(deliverBtn, new GUIContent("Deliver Button"));
                    break;
                case Lock.InteractionTypes.RequirePassword:
                    EditorGUILayout.PropertyField(passwordUI, new GUIContent("PasswordUI"));
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
            deliverOptionsContainer = serializedObject.FindProperty("_deliverOptionsContainer");
            navigateUIInput = serializedObject.FindProperty("_navigateUIInput");
            deliverBtn = serializedObject.FindProperty("_deliverBtn");
            passwordUI = serializedObject.FindProperty("_passwordUI");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
            onActivate = serializedObject.FindProperty("onActivate");
        }
    }
}
#endif