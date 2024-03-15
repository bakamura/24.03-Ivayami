#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Cinemachine;

namespace Paranapiacaba.Dialogue
{
    [CustomEditor(typeof(CameraTransitionInfo))]
    public class CameraTransitionInfoInspector : Editor
    {
        SerializedProperty cameraFinalPositionAndRotation, duration, positionCurve, rotationCurve;
        private Transform _dialogueCameraTransform;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(cameraFinalPositionAndRotation, new GUIContent("Camera Final Position And Rotation", "the final layout that the camera will be, if empty will use own transform"));
            EditorGUILayout.PropertyField(duration, new GUIContent("Duration"));
            EditorGUILayout.PropertyField(positionCurve, new GUIContent("Position Animation Blend"));
            EditorGUILayout.PropertyField(rotationCurve, new GUIContent("Rotation Animation Blend"));

            if (GUILayout.Button("CameraPreview"))
            {
                if (!_dialogueCameraTransform) _dialogueCameraTransform = FindObjectOfType<DialogueCamera>().GetComponentInChildren<CinemachineVirtualCamera>().transform;
                CameraTransitionInfo instance = (CameraTransitionInfo)target;
                _dialogueCameraTransform.SetPositionAndRotation(instance.GetValidTransform().position, instance.GetValidTransform().rotation);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cameraFinalPositionAndRotation = serializedObject.FindProperty("_cameraFinalPositionAndRotation");
            duration = serializedObject.FindProperty("duration");
            positionCurve = serializedObject.FindProperty("positionCurve");
            rotationCurve = serializedObject.FindProperty("rotationCurve");
        }
    }
}
#endif