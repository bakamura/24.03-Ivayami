#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Paranapiacaba.Puzzle
{
    [CustomEditor(typeof(FuseBox))]
    public class FusionBoxInspector : Editor
    {
        SerializedProperty matrixDimensions, distanceBetweenFuses, fuseObjectsParent, fuseUIParent, changeFuseInput, activateFuseInput, 
            fuseLayer, onInteract, onInteractionCancelled, /*inputActionMap,*/ onActivate, selectedColor, activatedColor, deactivatedColor, cancelInteractionInput;

        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            //EditorGUILayout.PropertyField(inputActionMap, new GUIContent("Input Action Map"));
            EditorGUILayout.PropertyField(changeFuseInput, new GUIContent("Select Fuse Input Reference"));
            EditorGUILayout.PropertyField(activateFuseInput, new GUIContent("Activate Fuse Input Reference"));
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Exit Puzzle Input Reference"));
            EditorGUILayout.Space(10);

            GUILayout.Label("FUSE BOX SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(matrixDimensions, new GUIContent("Fuse Box Dimensions"));
            EditorGUILayout.PropertyField(distanceBetweenFuses, new GUIContent("Distance Between Fuses"));
            EditorGUILayout.PropertyField(fuseLayer, new GUIContent("Fuse Collision Layer"));
            EditorGUILayout.PropertyField(selectedColor, new GUIContent("Fuse Selected Color"));
            EditorGUILayout.PropertyField(activatedColor, new GUIContent("Fuse Activated Color"));
            EditorGUILayout.PropertyField(deactivatedColor, new GUIContent("Fuse Deactivated Color"));
            EditorGUILayout.Space(10);

            GUILayout.Label("COMPONENTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(fuseObjectsParent, new GUIContent("Fuse Box Transform"));
            EditorGUILayout.PropertyField(fuseUIParent, new GUIContent("Fuse Box UI Transform"));
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onInteractionCancelled, new GUIContent("On Exit Interaction"));
            EditorGUILayout.PropertyField(onActivate, new GUIContent("On Activate"));

            FuseBox instance = (FuseBox)target;
            if (GUILayout.Button("Reposition Fuses"))
            {
                instance.RepositionFuses();
            }

            if (GUILayout.Button("Update Fuses"))
            {
                instance.UpdateFusesActiveState();
                instance.RenameObjects();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            matrixDimensions = serializedObject.FindProperty("_matrixDimensions");
            distanceBetweenFuses = serializedObject.FindProperty("_distanceBetweenFuses");
            fuseObjectsParent = serializedObject.FindProperty("_fuseObjectsParent");
            fuseUIParent = serializedObject.FindProperty("_fuseUIParent");
            changeFuseInput = serializedObject.FindProperty("_changeFuseInput");
            activateFuseInput = serializedObject.FindProperty("_activateFuseInput");
            fuseLayer = serializedObject.FindProperty("_fuseLayer");
            onInteract = serializedObject.FindProperty("_onInteract");
            onInteractionCancelled = serializedObject.FindProperty("_onInteractionCancelled");
            //inputActionMap = serializedObject.FindProperty("_inputActionMap");
            onActivate = serializedObject.FindProperty("onActivate");
            selectedColor = serializedObject.FindProperty("_selectedColor");
            activatedColor = serializedObject.FindProperty("_activatedColor");
            deactivatedColor = serializedObject.FindProperty("_deactivatedColor");
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
        }
    }
}
#endif