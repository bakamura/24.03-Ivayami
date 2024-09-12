using UnityEngine;
using UnityEditor;

namespace Ivayami.Audio 
{
    [CustomEditor(typeof(SoundEffectTrigger))]
    internal sealed class SoundEffectTriggerInspector : Editor
    {
        SerializedProperty audiosData, playOnStart, replayAudioOnEnd, replayIntervalRange;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(audiosData, new GUIContent("Audios To Play"));
            EditorGUILayout.PropertyField(playOnStart, new GUIContent("Play On Start"));
            EditorGUILayout.PropertyField(replayAudioOnEnd, new GUIContent("Replay Audio On End"));
            if (replayAudioOnEnd.boolValue)
                EditorGUILayout.PropertyField(replayIntervalRange, new GUIContent("Interval Random Range"));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            audiosData = serializedObject.FindProperty("_audiosData");
            playOnStart = serializedObject.FindProperty("_playOnStart");
            replayAudioOnEnd = serializedObject.FindProperty("_replayAudioOnEnd");
            replayIntervalRange = serializedObject.FindProperty("_replayIntervalRange");
        }
    }
}