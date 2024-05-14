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
        private List<DialogueSearchInfo> _dialoguesSearchCache = new List<DialogueSearchInfo>();
        private List<SceneSearchInfo> _sceneSearchCache = new List<SceneSearchInfo>();
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

        [System.Serializable]
        private struct SceneSearchInfo
        {
            public string SceneName;
            public string ObjectName;

            public SceneSearchInfo(string sceneName, string objectName)
            {
                SceneName = sceneName;
                ObjectName = objectName;
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
            _dialoguesSearchCache.Clear();
            _sceneSearchCache.Clear();
            switch (_currentSearchType)
            {
                case SearchType.Event:
                    //collect object name and scene with the event
                    string folder = Path.Combine(Application.dataPath, "_Game", "Scene");
                    string[] files = Directory.GetFiles(folder, "*.unity", SearchOption.AllDirectories);
                    string currentGobjName = null;
                    for (int i = 0; i < files.Length; i++)
                    {
                        foreach (var line in File.ReadAllLines(files[i]))
                        {
                            if (line.Contains("m_Name: ")) currentGobjName = line.Split(' ').ToString();
                            if (line.Contains(_filter))
                            {
                                _sceneSearchCache.Add(new SceneSearchInfo(Path.GetFileName(files[i]), currentGobjName));
                                break;
                            }
                            //if (line.Contains(_filter) && current == null)
                            //    current = new List<string>();
                            //else if (line.Contains(_filter) && current != null)
                            //{
                            //    groups.Add(current);
                            //    current = null;
                            //}
                            //if (current != null)
                            //    current.Add(line);
                        }
                    }
                    //collect dialogue assets with the events
                    for (int i = 0; i < _allDialogues.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(_allDialogues[i].onEndEventId) && _allDialogues[i].onEndEventId.ToUpper().Contains(_filter.ToUpper()))
                        {
                            _dialoguesSearchCache.Add(new DialogueSearchInfo(_allDialogues[i], -1));
                        }
                        for (int a = 0; a < _allDialogues[i].dialogue.Length; a++)
                        {
                            if (!string.IsNullOrEmpty(_allDialogues[i].dialogue[a].eventId) && _allDialogues[i].dialogue[a].eventId.ToUpper().Contains(_filter.ToUpper()))
                            {
                                _dialoguesSearchCache.Add(new DialogueSearchInfo(_allDialogues[i], (sbyte)a));
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
                                _dialoguesSearchCache.Add(new DialogueSearchInfo(_allDialogues[i], (sbyte)a));
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
                    DrawSceneResult(_buttonRect);
                    temp = GUILayoutUtility.GetLastRect();
                    //draw dialogue data
                    DrawDialogueResult(temp);
                    break;
                case SearchType.Filter:
                    DrawDialogueResult(temp);
                    break;
            }
        }

        private void DrawSceneResult(Rect rect)
        {

        }

        private void DrawDialogueResult(Rect rect)
        {
            float baseXPos = rect.x;
            rect.width /= 2;
            for (int i = 0; i < _dialoguesSearchCache.Count; i++)
            {
                rect.y += rect.height * 1.1f;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.ObjectField(rect, _dialoguesSearchCache[i].Dialogue, typeof(Dialogue), false);
                rect.x += rect.width * 1.1f;
                if (_dialoguesSearchCache[i].SpeechIndex == -1) EditorGUI.LabelField(rect, "On End Event");
                else EditorGUI.LabelField(rect, $"Speech Index: {i}");
                EditorGUILayout.EndHorizontal();
                rect.x = baseXPos;
            }
        }
    }
}
#endif