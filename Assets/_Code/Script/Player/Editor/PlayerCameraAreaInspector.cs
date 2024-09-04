#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ivayami.Player
{
    [CustomEditor(typeof(PlayerCameraArea))]
    internal sealed class PlayerCameraAreaInspector : Editor
    {
        SerializedProperty cameraDistance, changeCameraRadius, camerasRadius, radiusLerpDuration;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(cameraDistance, new GUIContent("Camera Distance"));
            EditorGUILayout.PropertyField(changeCameraRadius, new GUIContent("Change Camera Radius"));
            if (changeCameraRadius.boolValue)
            {
                EditorGUILayout.PropertyField(camerasRadius, new GUIContent("Camera Radius"));
                EditorGUILayout.PropertyField(radiusLerpDuration, new GUIContent("Radius Lerp Duration"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cameraDistance = serializedObject.FindProperty("_cameraDistance");
            changeCameraRadius = serializedObject.FindProperty("_changeCameraRadius");
            camerasRadius = serializedObject.FindProperty("_camerasRadius");
            radiusLerpDuration = serializedObject.FindProperty("_radiusLerpDuration");
        }
    }
}
#endif