using UnityEngine;
using UnityEditor;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(DeliverUI))]
    public class DeliverUIInspector : Editor
    {
        SerializedProperty navigateUIInput, requestAmountToComplete, skipDeliverUI, deliverAnyItem, itemsRequired, deliverItemOptionsIcon, itemDisplayName, deliverBtn, onTryDeliver;
        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(navigateUIInput, new GUIContent("Navigate UI Input"));
            EditorGUILayout.Space(10);

            GUILayout.Label("CONFIGURATIONS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(requestAmountToComplete, new GUIContent("Request Amount To complete"));
            if (!deliverAnyItem.boolValue) EditorGUILayout.PropertyField(skipDeliverUI, new GUIContent("Skip Deliver UI"));
            if (!skipDeliverUI.boolValue) EditorGUILayout.PropertyField(deliverAnyItem, new GUIContent("Deliver Any Item"));
            EditorGUILayout.PropertyField(itemsRequired, new GUIContent("Items Required"));
            EditorGUILayout.Space(10);

            GUILayout.Label("COMPONENTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(deliverBtn, new GUIContent("Deliver Button"));
            EditorGUILayout.PropertyField(deliverItemOptionsIcon, new GUIContent("Deliver Item Options Icon"));
            EditorGUILayout.PropertyField(itemDisplayName, new GUIContent("Item Display Name Text"));
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onTryDeliver, new GUIContent("On Try Deliver"));
            EditorGUILayout.Space(5);

            if (deliverAnyItem.boolValue) skipDeliverUI.boolValue = false;
            if (skipDeliverUI.boolValue) deliverAnyItem.boolValue = false;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            navigateUIInput = serializedObject.FindProperty("_navigateUIInput");
            requestAmountToComplete = serializedObject.FindProperty("_requestAmountToComplete");
            skipDeliverUI = serializedObject.FindProperty("_skipDeliverUI");
            deliverAnyItem = serializedObject.FindProperty("_deliverAnyItem");
            itemsRequired = serializedObject.FindProperty("_itemsRequired");
            deliverItemOptionsIcon = serializedObject.FindProperty("_deliverItemOptionsIcon");
            itemDisplayName = serializedObject.FindProperty("_itemDisplayName");
            deliverBtn = serializedObject.FindProperty("_deliverBtn");
            onTryDeliver = serializedObject.FindProperty("_onTryDeliver");
        }
    }
}
