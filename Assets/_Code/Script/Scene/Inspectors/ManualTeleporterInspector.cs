#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ivayami.Scene;

[CustomEditor(typeof(Ivayami.Scene.ManualTeleporter))]
internal sealed class ManualTeleporterInspector : Editor
{
    SerializedProperty teleportType, teleportTarget, gizmoColor, gizmoSize;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("PARAMETERS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(teleportType, new GUIContent("Teleport Type"));
        if ((ManualTeleporter.TeleportTypes)teleportType.enumValueIndex == ManualTeleporter.TeleportTypes.Object)
            EditorGUILayout.PropertyField(teleportTarget, new GUIContent("Teleport Target"));

        EditorGUILayout.LabelField("DEBUG", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(gizmoColor, new GUIContent("Gizmo Color"));

        if ((ManualTeleporter.TeleportTypes)teleportType.enumValueIndex == ManualTeleporter.TeleportTypes.Object)
            EditorGUILayout.PropertyField(gizmoSize, new GUIContent("Gizmo Size"));

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        teleportType = serializedObject.FindProperty("_teleportType");
        teleportTarget = serializedObject.FindProperty("_teleportTarget");
        gizmoColor = serializedObject.FindProperty("_gizmoColor");
        gizmoSize = serializedObject.FindProperty("_gizmoSize");
    }
}
#endif