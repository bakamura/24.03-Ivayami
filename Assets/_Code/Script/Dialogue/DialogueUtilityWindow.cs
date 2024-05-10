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
        private Rect _buttonRect;
        private List<Dialogue> _dialoguesCache = new List<Dialogue>();
        private SearchType _currentSearchType;
        private enum SearchType
        {
            Event,
            Filter
        }

        [MenuItem("Ivayami/DialogueUtilities")]
        private static void ShowWindow()
        {
            var window = GetWindow<DialogueUtilityWindow>();
            window.titleContent = new GUIContent("Dialogue Utilities");
            window.Show();
        }

        private void OnGUI()
        {
            _currentSearchType = (SearchType)EditorGUILayout.EnumPopup("Search Type", _currentSearchType);
            _filter = EditorGUILayout.TextField("Search", _filter);

            _buttonRect = GUILayoutUtility.GetLastRect();
            _buttonRect.y += _buttonRect.height * 1.2f;
            if (!string.IsNullOrEmpty(_filter) && GUI.Button(_buttonRect, "Search"))
            {                
                DrawSearchResult();                
            }
        }

        private void DrawSearchResult()
        {
            switch (_currentSearchType)
            {
                case SearchType.Event:
                    break;
                case SearchType.Filter:
                    _allDialogues = Resources.LoadAll<Dialogue>("Dialogues");
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
                    Rect temp = GUILayoutUtility.GetLastRect();
                    for (int i = 0; i < _dialoguesCache.Count; i++)
                    {
                        temp.y = temp.height * 1.2f;
                        EditorGUI.ObjectField(temp, _dialoguesCache[i], typeof(Dialogue), false);
                    }
                    break;
            }            
        }
    }
}
#endif