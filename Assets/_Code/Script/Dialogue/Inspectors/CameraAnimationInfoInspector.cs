#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Cinemachine;

namespace Paranapiacaba.Dialogue
{
    [CustomEditor(typeof(CameraAnimationInfo))]
    public class CameraAnimationInfoInspector : Editor
    {
        SerializedProperty duration, positionCurve, rotationCurve, willBeInDialogue;
        private Transform _dialogueCameraTransform;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(duration, new GUIContent("Duration"));
            EditorGUILayout.PropertyField(positionCurve, new GUIContent("Position Animation Blend"));
            EditorGUILayout.PropertyField(rotationCurve, new GUIContent("Rotation Animation Blend"));
            EditorGUILayout.PropertyField(willBeInDialogue, new GUIContent("Will Happen inside Dialogue"));

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
            duration = serializedObject.FindProperty("duration");
            positionCurve = serializedObject.FindProperty("positionCurve");
            rotationCurve = serializedObject.FindProperty("rotationCurve");
            willBeInDialogue = serializedObject.FindProperty("_willBeInDialogue");
        }
    }
}
#endif