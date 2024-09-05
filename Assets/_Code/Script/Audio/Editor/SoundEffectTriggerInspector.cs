using UnityEngine;
using UnityEditor;

namespace Ivayami.Audio 
{
    [CustomEditor(typeof(SoundEffectTrigger))]
    internal sealed class SoundEffectTriggerInspector : Editor
    {
        SerializedProperty audioToPlay, playOnStart, replayAudioOnEnd, replayIntervalRange;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(audioToPlay, new GUIContent("Audio To Play"));
            EditorGUILayout.PropertyField(playOnStart, new GUIContent("Play On Start"));
            EditorGUILayout.PropertyField(replayAudioOnEnd, new GUIContent("Replay Audio On End"));
            if (replayAudioOnEnd.boolValue)
                EditorGUILayout.PropertyField(replayIntervalRange, new GUIContent("Interval Random Range"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            audioToPlay = serializedObject.FindProperty("_audioToPlay");
            playOnStart = serializedObject.FindProperty("_playOnStart");
            replayAudioOnEnd = serializedObject.FindProperty("_replayAudioOnEnd");
            replayIntervalRange = serializedObject.FindProperty("_replayIntervalRange");
        }
    }
}