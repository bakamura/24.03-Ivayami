using UnityEngine;
using UnityEditor;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(InteractableButton))]
    public class InteractableButtonInspector : Editor
    {
        SerializedProperty animationType, startActive, playAudioOnStart, onActivate;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(animationType, new GUIContent("Animation Type", "The animation that the player will have when an interaction occurs"));
            EditorGUILayout.PropertyField(startActive, new GUIContent("Start On Active State"));
            if (startActive.boolValue) EditorGUILayout.PropertyField(playAudioOnStart, new GUIContent("Play Audio On Start"));
            EditorGUILayout.PropertyField(onActivate, new GUIContent("On Activate"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            animationType = serializedObject.FindProperty("_animationType");
            startActive = serializedObject.FindProperty("_startActive");
            playAudioOnStart = serializedObject.FindProperty("_playAudioOnStart");
            onActivate = serializedObject.FindProperty("onActivate");
        }
    }
}