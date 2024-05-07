#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ivayami.Dialogue
{
    internal sealed class DialogueUtilityWindow : EditorWindow
    {
        private Dialogue[] _allDialogues;
        private string _filter;
        private Vector2 _scrollPosition;

        [MenuItem("Ivayami/DialogueUtilities")]
        private static void ShowWindow()
        {
            var window = GetWindow<DialogueUtilityWindow>();
            window.titleContent = new GUIContent("Dialogue Utilities");
            window.Show();
        }

        private void OnGUI()
        {
            _allDialogues = Resources.LoadAll<Dialogue>("Dialogues");

            _filter = EditorGUILayout.TextField("Filter", _filter);

            DrawSearchResult();
        }

        private void DrawSearchResult()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        }
    }
}
#endif