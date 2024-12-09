using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ivayami.Dialogue
{
    internal sealed class DialogueActorWindow : EditorWindow
    {
        private SerializedProperty actors;
        private SerializedObject _instance;

        [MenuItem("Ivayami/DialogueUtilities/Actor Table")]
        private static void ShowWindow()
        {
            var window = GetWindow<DialogueActorWindow>();
            window.titleContent = new GUIContent("Actor Table");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(actors, new GUIContent("Actors"));
            _instance.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _instance = new SerializedObject(Resources.Load<DialogueActor>("Dialogues/ActorsTable"));
            actors = _instance.FindProperty("Actors");
        }
    }
}