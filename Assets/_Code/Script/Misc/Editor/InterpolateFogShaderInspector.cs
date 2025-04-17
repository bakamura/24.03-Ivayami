using UnityEngine;
using UnityEditor;
using Ivayami.Misc;

[CustomEditor(typeof(InterpolateFogShader))]
internal sealed class InterpolateFogShaderInspector : Editor
{
    SerializedProperty interpolationCurve, duration, sphericalFogFinalValue, psxFogFinalValue, changeColor, sphericalFogFinalColor, psxFogFinalColor;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(interpolationCurve, new GUIContent("Interpolation Curve"));
        EditorGUILayout.PropertyField(duration, new GUIContent("Duration"));
        EditorGUILayout.PropertyField(sphericalFogFinalValue, new GUIContent("Spherical Fog Final Value"));
        EditorGUILayout.PropertyField(psxFogFinalValue, new GUIContent("PSX Fog Final Value"));
        EditorGUILayout.PropertyField(changeColor, new GUIContent("Change Color"));
        if (changeColor.boolValue)
        {
            EditorGUILayout.PropertyField(sphericalFogFinalColor, new GUIContent("Spherical Fog Final Color"));
            EditorGUILayout.PropertyField(psxFogFinalColor, new GUIContent("PSX Fog Final Color"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        interpolationCurve = serializedObject.FindProperty("_interpolationCurve");
        duration = serializedObject.FindProperty("_duration");
        sphericalFogFinalValue = serializedObject.FindProperty("_sphericalFogFinalValue");
        psxFogFinalValue = serializedObject.FindProperty("_psxFogFinalValue");
        changeColor = serializedObject.FindProperty("_changeColor");
        sphericalFogFinalColor = serializedObject.FindProperty("_sphericalFogFinalColor");
        psxFogFinalColor = serializedObject.FindProperty("_psxFogFinalColor");
    }
}
