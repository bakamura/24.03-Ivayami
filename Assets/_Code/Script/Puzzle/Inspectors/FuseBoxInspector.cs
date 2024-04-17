#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Paranapiacaba.Puzzle
{
    [CustomEditor(typeof(FuseBox))]
    public class FuseBoxInspector : Editor
    {
        SerializedProperty matrixDimensions, distanceBetweenLeds, fusesParent, fuseUIParent, changeFuseInput, activateFuseInput, 
            fuseLayer, onInteract, onInteractionCancelled, onActivate, selectedColor, activatedColor, cancelInteractionInput, fusePrefab,
            ledsParent, fusesOffset, ledPrefab, elementsOffset, deactivatedColor;

        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(changeFuseInput, new GUIContent("Select Fuse Input Reference"));
            EditorGUILayout.PropertyField(activateFuseInput, new GUIContent("Activate Fuse Input Reference"));
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Exit Puzzle Input Reference"));
            EditorGUILayout.Space(10);

            GUILayout.Label("FUSE BOX SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(matrixDimensions, new GUIContent("Fuse Box Dimensions"));
            EditorGUILayout.PropertyField(distanceBetweenLeds, new GUIContent("Distance Between Leds"));
            EditorGUILayout.PropertyField(fusesOffset, new GUIContent("Fuse Container Offset"));
            EditorGUILayout.PropertyField(elementsOffset, new GUIContent("All Elements Offset"));
            EditorGUILayout.PropertyField(fuseLayer, new GUIContent("Fuse Collision Layer"));
            EditorGUILayout.PropertyField(selectedColor, new GUIContent("Fuse Selected Color"));
            EditorGUILayout.PropertyField(activatedColor, new GUIContent("Led Activated Color"));
            EditorGUILayout.PropertyField(deactivatedColor, new GUIContent("Led Deactivated Color"));
            EditorGUILayout.Space(10);

            GUILayout.Label("COMPONENTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(fusesParent, new GUIContent("Fuse Container"));
            EditorGUILayout.PropertyField(fuseUIParent, new GUIContent("Fuse UI Container"));
            EditorGUILayout.PropertyField(fusePrefab, new GUIContent("Fuse Prefab"));
            EditorGUILayout.PropertyField(ledsParent, new GUIContent("Led Container"));
            EditorGUILayout.PropertyField(ledPrefab, new GUIContent("Led Prefab"));
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onInteractionCancelled, new GUIContent("On Exit Interaction"));
            EditorGUILayout.PropertyField(onActivate, new GUIContent("On Activate"));

            FuseBox instance = (FuseBox)target;

            if (GUILayout.Button("Update Fuses"))
            {
                instance.CreateFuses();
                instance.RepositionFuses();
                instance.UpdateFusesActiveState();
                instance.RenameObjects();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            matrixDimensions = serializedObject.FindProperty("_matrixDimensions");
            distanceBetweenLeds = serializedObject.FindProperty("_distanceBetweenLeds");
            fusesParent = serializedObject.FindProperty("_fusesParent");
            fuseUIParent = serializedObject.FindProperty("_fuseUIParent");
            changeFuseInput = serializedObject.FindProperty("_changeFuseInput");
            activateFuseInput = serializedObject.FindProperty("_activateFuseInput");
            fuseLayer = serializedObject.FindProperty("_fuseLayer");
            onInteract = serializedObject.FindProperty("_onInteract");
            onInteractionCancelled = serializedObject.FindProperty("_onInteractionCancelled");
            onActivate = serializedObject.FindProperty("onActivate");
            selectedColor = serializedObject.FindProperty("_selectedColor");
            activatedColor = serializedObject.FindProperty("_activatedColor");
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            fusePrefab = serializedObject.FindProperty("_fusePrefab");
            ledsParent = serializedObject.FindProperty("_ledsParent");
            fusesOffset = serializedObject.FindProperty("_fusesOffset");
            ledPrefab = serializedObject.FindProperty("_ledPrefab");
            elementsOffset = serializedObject.FindProperty("_elementsOffset");
            deactivatedColor = serializedObject.FindProperty("_deactivatedColor");
        }
    }
}
#endif