#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ivayami.Player
{
    [CustomEditor(typeof(PlayerCameraArea))]
    internal sealed class PlayerCameraAreaInspector : Editor
    {
        SerializedProperty cameraDistance, changeCameraRadius, camerasRadius, radiusLerpDuration, changeCameraHeight, camerasHeight, heightLerpDuration;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(cameraDistance, new GUIContent("Camera Distance"));
            EditorGUILayout.PropertyField(changeCameraRadius, new GUIContent("Change Camera Radius"));
            if (changeCameraRadius.boolValue)
            {
                EditorGUILayout.PropertyField(camerasRadius, new GUIContent("Camera Radius"));
                EditorGUILayout.PropertyField(radiusLerpDuration, new GUIContent("Radius Lerp Duration"));
            }
            EditorGUILayout.PropertyField(changeCameraHeight, new GUIContent("Change Camera Height"));
            if (changeCameraHeight.boolValue)
            {
                EditorGUILayout.PropertyField(camerasHeight, new GUIContent("Camera Heights"));
                EditorGUILayout.PropertyField(heightLerpDuration, new GUIContent("Heights Lerp Duration"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cameraDistance = serializedObject.FindProperty("_cameraDistance");
            changeCameraRadius = serializedObject.FindProperty("_changeCameraRadius");
            camerasRadius = serializedObject.FindProperty("_camerasRadius");
            radiusLerpDuration = serializedObject.FindProperty("_radiusLerpDuration");
            changeCameraHeight = serializedObject.FindProperty("_changeCameraHeight");
            camerasHeight = serializedObject.FindProperty("_camerasHeight");
            heightLerpDuration = serializedObject.FindProperty("_heightLerpDuration");
        }
    }
}
#endif