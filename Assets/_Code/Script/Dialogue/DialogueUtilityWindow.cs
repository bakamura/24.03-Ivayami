#if UNITY_EDITOR
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
        private List<DialogueSearchInfo> _dialoguesCache = new List<DialogueSearchInfo>();
        private SearchType _currentSearchType;
        private enum SearchType
        {
            Event,
            Filter
        }

        [System.Serializable]        
        private struct DialogueSearchInfo
        {
            public Dialogue Dialogue; 
            /// <summary>
            /// if equals -1 the event is the OnEndEvent
            /// </summary>
            public sbyte SpeechIndex;

            public DialogueSearchInfo(Dialogue dialogue, sbyte speechIndex)
            {
                Dialogue = dialogue;
                SpeechIndex = speechIndex;
            }
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
            _allDialogues = Resources.LoadAll<Dialogue>("Dialogues");
            _dialoguesCache.Clear();
            Rect temp = _buttonRect;
            switch (_currentSearchType)
            {
                case SearchType.Event:
                    //draw object name and scene with the event
                    //draw dialogue assets with the events
                    for (int i = 0; i < _allDialogues.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(_allDialogues[i].onEndEventId) && _allDialogues[i].onEndEventId.ToUpper().Contains(_filter.ToUpper()))
                        {
                            _dialoguesCache.Add(new DialogueSearchInfo(_allDialogues[i], -1));
                        }
                        for (int a = 0; a < _allDialogues[i].dialogue.Length; a++)
                        {
                            if (!string.IsNullOrEmpty(_allDialogues[i].dialogue[a].eventId) && _allDialogues[i].dialogue[a].eventId.ToUpper().Contains(_filter.ToUpper()))
                            {
                                _dialoguesCache.Add(new DialogueSearchInfo(_allDialogues[i], (sbyte)a));
                            }
                        }
                    }
                    for (int i = 0; i < _dialoguesCache.Count; i++)
                    {
                        temp.y += temp.height * 1.1f;
                        Debug.Log(temp);
                        EditorGUI.ObjectField(temp, _dialoguesCache[i].Dialogue, typeof(Dialogue), false);
                        EditorGUILayout.BeginHorizontal();
                        if (_dialoguesCache[i].SpeechIndex == -1) EditorGUILayout.LabelField("On End Event");
                        else EditorGUILayout.LabelField($"Speech Index: {i}");
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                case SearchType.Filter:
                    for (int i = 0; i < _allDialogues.Length; i++)
                    {
                        for (int a = 0; a < _allDialogues[i].dialogue.Length; a++)
                        {
                            if (!string.IsNullOrEmpty(_allDialogues[i].dialogue[a].FilterTags) && _allDialogues[i].dialogue[a].FilterTags.ToUpper().Contains(_filter.ToUpper()))
                            {
                                _dialoguesCache.Add(new DialogueSearchInfo(_allDialogues[i], -1));
                            }
                        }
                    }
                    for (int i = 0; i < _dialoguesCache.Count; i++)
                    {
                        temp.y += temp.height * 1.1f;
                        Debug.Log(temp);
                        EditorGUI.ObjectField(temp, _dialoguesCache[i].Dialogue, typeof(Dialogue), false);
                    }
                    break;
            }
        }
    }
}
#endif