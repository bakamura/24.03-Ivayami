#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Cinemachine;

namespace Ivayami.Dialogue
{
    [CustomEditor(typeof(CameraAnimationInfo))]
    public class CameraAnimationInfoInspector : Editor
    {
        SerializedProperty duration, hidePlayerModel, positionCurve, rotationCurve,
            changeCameraFocus, lookAtPlayer, followPlayer, lookAtTarget, followTarget;
        private Transform _dialogueCameraTransform;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(duration, new GUIContent("Interpolation Duration"));
            EditorGUILayout.PropertyField(hidePlayerModel, new GUIContent("Hide Player Model", "Hide Player Model when this camera activates"));
            EditorGUILayout.PropertyField(changeCameraFocus, new GUIContent("Change Camera Focus", "If true the rotation and position of this object is Not relevant"));
            if (changeCameraFocus.boolValue)
            {
                EditorGUILayout.PropertyField(lookAtPlayer, new GUIContent("Look At Player"));
                if (!lookAtPlayer.boolValue) EditorGUILayout.PropertyField(lookAtTarget, new GUIContent("Look At Target Transform"));
                EditorGUILayout.PropertyField(followPlayer, new GUIContent("Follow Player"));
                if (!followPlayer.boolValue) EditorGUILayout.PropertyField(followTarget, new GUIContent("Follow Target Transform"));
            }
            EditorGUILayout.PropertyField(positionCurve, new GUIContent("Position Animation Blend"));
            EditorGUILayout.PropertyField(rotationCurve, new GUIContent("Rotation Animation Blend"));

            if (!changeCameraFocus.boolValue && GUILayout.Button("CameraPreview"))
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
            hidePlayerModel = serializedObject.FindProperty("_hidePlayerModel");
            changeCameraFocus = serializedObject.FindProperty("_changeCameraFocus");
            lookAtPlayer = serializedObject.FindProperty("_lookAtPlayer");
            followPlayer = serializedObject.FindProperty("_followPlayer");
            lookAtTarget = serializedObject.FindProperty("_lookAtTarget");
            followTarget = serializedObject.FindProperty("_followTarget");
        }
    }
}
#endif