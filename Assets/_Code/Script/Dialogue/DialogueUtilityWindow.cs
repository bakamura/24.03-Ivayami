#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using UnityEngine.SceneManagement;

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
                UpdateSearchResult();
            }
            DrawSearchResult();
        }

        private void UpdateSearchResult()
        {
            _allDialogues = Resources.LoadAll<Dialogue>("Dialogues");
            _dialoguesCache.Clear();
            switch (_currentSearchType)
            {
                case SearchType.Event:
                    //collect object name and scene with the event
                    //string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Archive\";
                    //string filter = "*.unity";
                    //string[] files = Directory.GetFiles(folder, filter);
                    //List<string> fileNames = new List<string>();
                    //for(int i = 0; i < files.Length; i++)
                    //{
                    //    fileNames.Add(Path.GetFileNameWithoutExtension(folder));
                    //}                    
                    //List<List<string>> groups = new List<List<string>>();
                    //List<string> current = null;
                    //foreach (var line in File.ReadAllLines(pathToFile))
                    //{
                    //    if (line.Contains("CustomerEN") && current == null)
                    //        current = new List<string>();
                    //    else if (line.Contains("CustomerCh") && current != null)
                    //    {
                    //        groups.Add(current);
                    //        current = null;
                    //    }
                    //    if (current != null)
                    //        current.Add(line);
                    //}
                    //collect dialogue assets with the events
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
                    break;
                case SearchType.Filter:
                    for (int i = 0; i < _allDialogues.Length; i++)
                    {
                        for (int a = 0; a < _allDialogues[i].dialogue.Length; a++)
                        {
                            if (!string.IsNullOrEmpty(_allDialogues[i].dialogue[a].FilterTags) && _allDialogues[i].dialogue[a].FilterTags.ToUpper().Contains(_filter.ToUpper()))
                            {
                                _dialoguesCache.Add(new DialogueSearchInfo(_allDialogues[i], (sbyte)a));
                            }
                        }
                    }
                    break;
            }
        }

        private void DrawSearchResult()
        {
            Rect temp = _buttonRect;            
            switch (_currentSearchType)
            {
                case SearchType.Event:
                    //draw scene data
                    //draw dialogue data
                    DrawDialogueResult(temp);
                    break;
                case SearchType.Filter:
                    DrawDialogueResult(temp);
                    break;
            }
        }
        private void DrawDialogueResult(Rect rect)
        {
            float baseXPos = rect.x;
            rect.width /= 2;
            for (int i = 0; i < _dialoguesCache.Count; i++)
            {
                rect.y += rect.height * 1.1f;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.ObjectField(rect, _dialoguesCache[i].Dialogue, typeof(Dialogue), false);
                rect.x += rect.width * 1.1f;
                if (_dialoguesCache[i].SpeechIndex == -1) EditorGUI.LabelField(rect, "On End Event");
                else EditorGUI.LabelField(rect, $"Speech Index: {i}");
                EditorGUILayout.EndHorizontal();
                rect.x = baseXPos;
            }
        }
    }
}
#endif