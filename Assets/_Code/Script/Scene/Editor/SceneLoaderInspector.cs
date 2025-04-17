using UnityEngine;
using UnityEditor;
using Ivayami.Scene;

[CustomEditor(typeof(SceneLoader))]
internal sealed class SceneLoaderInspector : Editor
{
    SerializedProperty sceneId, changeSkybox, backgroundType, backgroundColor, onSceneLoad, onSceneUnload, onAllScenesRequestEnd, drawGizmos, gizmoColor;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(sceneId, new GUIContent("Scene ID"));
        EditorGUILayout.PropertyField(changeSkybox, new GUIContent("Change Skybox"));
        if (changeSkybox.boolValue)
        {
            EditorGUILayout.PropertyField(backgroundType, new GUIContent("Background Type"));
            if (backgroundType.enumValueFlag == 2) EditorGUILayout.PropertyField(backgroundColor, new GUIContent("Background Color"));
        }
        EditorGUILayout.PropertyField(onSceneLoad, new GUIContent("On Scene Load"));
        EditorGUILayout.PropertyField(onSceneUnload, new GUIContent("On Scene Unload"));
        EditorGUILayout.PropertyField(onAllScenesRequestEnd, new GUIContent("On All Scenes Request End"));
        EditorGUILayout.PropertyField(drawGizmos, new GUIContent("Draw Gizmos"));
        EditorGUILayout.PropertyField(gizmoColor, new GUIContent("Gizmo Color"));

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        sceneId = serializedObject.FindProperty("_sceneId");
        changeSkybox = serializedObject.FindProperty("_changeSkybox");
        backgroundType = serializedObject.FindProperty("_backgroundType");
        backgroundColor = serializedObject.FindProperty("_backgroundColor");
        onSceneLoad = serializedObject.FindProperty("_onSceneLoad");
        onSceneUnload = serializedObject.FindProperty("_onSceneUnload");
        onAllScenesRequestEnd = serializedObject.FindProperty("_onAllScenesRequestEnd");
        drawGizmos = serializedObject.FindProperty("_drawGizmos");
        gizmoColor = serializedObject.FindProperty("_gizmoColor");
    }
}
