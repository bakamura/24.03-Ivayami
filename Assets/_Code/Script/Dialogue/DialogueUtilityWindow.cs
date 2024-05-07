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
        private Rect _buttonRect;
        private List<Dialogue> _dialoguesCache = new List<Dialogue>();

        [MenuItem("Ivayami/DialogueUtilities")]
        private static void ShowWindow()
        {
            var window = GetWindow<DialogueUtilityWindow>();
            window.titleContent = new GUIContent("Dialogue Utilities");
            window.Show();
        }

        private void OnGUI()
        {
            _filter = EditorGUILayout.TextField("Filter", _filter);

            _buttonRect = GUILayoutUtility.GetLastRect();
            _buttonRect.y += _buttonRect.height * 1.2f;
            if (!string.IsNullOrEmpty(_filter) && GUI.Button(_buttonRect, "Search"))
            {
                DrawSearchResult();
            }
        }

        private void DrawSearchResult()
        {
            _allDialogues = Resources.LoadAll<Dialogue>("Dialogues");
            //Rect temp = GUILayoutUtility.GetLastRect();
            //show transform using the events
            //show dialogue assets

            for (int i = 0; i < _allDialogues.Length; i++)
            {
                for (int a = 0; a < _allDialogues[i].dialogue.Length; a++)
                {
                    if (!string.IsNullOrEmpty(_allDialogues[i].dialogue[a].FilterTags) && _allDialogues[i].dialogue[a].FilterTags.Contains(_filter))
                    {
                        _dialoguesCache.Add(_allDialogues[i]);
                    }
                }
            }
            //temp.y += temp.height * 1.2f;            
            EditorGUILayout.Foldout(true, GUIContent.none);
            for(int i = 0; i < _dialoguesCache.Count; i++)
            {
                EditorGUILayout.ObjectField(_dialoguesCache[i], typeof(Dialogue), false);
            }
        }
    }
}
#endif