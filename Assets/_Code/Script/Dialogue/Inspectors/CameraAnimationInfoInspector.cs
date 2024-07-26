#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Cinemachine;

namespace Ivayami.Dialogue
{
    [CustomEditor(typeof(CameraAnimationInfo))]
    public class CameraAnimationInfoInspector : Editor
    {
        SerializedProperty duration, positionCurve, rotationCurve;
        private Transform _dialogueCameraTransform;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(duration, new GUIContent("Duration"));
            EditorGUILayout.PropertyField(positionCurve, new GUIContent("Position Animation Blend"));
            EditorGUILayout.PropertyField(rotationCurve, new GUIContent("Rotation Animation Blend"));

            if (GUILayout.Button("CameraPreview"))
            {
                if (!_dialogueCameraTransform) _dialogueCameraTransform = FindObjectOfType<DialogueCamera>().GetComponentInChildren<CinemachineVirtualCamera>().transform;
                CameraAnimationInfo instance = (CameraAnimationInfo)target;
                _dialogueCameraTransform.SetPositionAndRotation(instance.transform.position, instance.transform.rotation);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            duration = serializedObject.FindProperty("Duration");
            positionCurve = serializedObject.FindProperty("PositionCurve");
            rotationCurve = serializedObject.FindProperty("RotationCurve");
        }
    }
}
#endif