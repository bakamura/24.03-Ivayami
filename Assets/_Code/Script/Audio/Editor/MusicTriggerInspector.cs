using UnityEngine;
using UnityEditor;
using Ivayami.Audio;

[CustomEditor(typeof(MusicTrigger))]
internal sealed class MusicTriggerInspector : Editor
{
    SerializedProperty music, forceStopCurrentMusicOnEnter, useDefaultFade, fadeDuration, useDefaultStartDelay, startDelay, shouldStopPeriodicaly, useDefaultReplay, replay;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(music, new GUIContent("Music"));
        EditorGUILayout.PropertyField(forceStopCurrentMusicOnEnter, new GUIContent("Force Stop Current Music On Enter"));

        EditorGUILayout.PropertyField(useDefaultFade, new GUIContent("Use Default Fade", "The value is defined in the Music Prefab"));
        if (!useDefaultFade.boolValue) EditorGUILayout.PropertyField(fadeDuration, new GUIContent("Fade Duration", "Min is Fade In, Max is Fade Out"));

        EditorGUILayout.PropertyField(useDefaultStartDelay, new GUIContent("Use Default Start Delay", "The value is defined in the Music Prefab"));
        if (!useDefaultStartDelay.boolValue) EditorGUILayout.PropertyField(startDelay, new GUIContent("Start Delay"));

        EditorGUILayout.PropertyField(shouldStopPeriodicaly, new GUIContent("Should Stop Periodicaly"));
        if (shouldStopPeriodicaly.boolValue)
        {
            EditorGUILayout.PropertyField(useDefaultReplay, new GUIContent("Use Default Relay", "The value is defined in the Music Prefab"));
            if (!useDefaultReplay.boolValue) EditorGUILayout.PropertyField(replay, new GUIContent("Replay"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        music = serializedObject.FindProperty("_music");
        forceStopCurrentMusicOnEnter = serializedObject.FindProperty("_forceStopCurrentMusicOnEnter");
        useDefaultFade = serializedObject.FindProperty("_useDefaultFade");
        fadeDuration = serializedObject.FindProperty("_fadeDuration");
        useDefaultStartDelay = serializedObject.FindProperty("_useDefaultStartDelay");
        startDelay = serializedObject.FindProperty("_startDelay");
        shouldStopPeriodicaly = serializedObject.FindProperty("_shouldStopPeriodicaly");
        useDefaultReplay = serializedObject.FindProperty("_useDefaultReplay");
        replay = serializedObject.FindProperty("_replay");
    }
}
