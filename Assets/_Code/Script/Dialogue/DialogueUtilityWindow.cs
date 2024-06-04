#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System;

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
        private string _objectNameToSelect;
        private string[] _filesCache;
        private enum SearchType
        {
            Event,
            Filter
        }

        [Serializable]
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

        [Serializable]
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
                    _filesCache = Directory.GetFiles(Path.Combine(Application.dataPath, "_Game", "Scene"),
                        "*.unity", SearchOption.AllDirectories);
                    string currentGobjName = null;
                    string splitResult;
                    for (int i = 0; i < _filesCache.Length; i++)
                    {
                        foreach (var line in File.ReadAllLines(_filesCache[i]))
                        {
                            if (line.Contains("m_Name:"))
                            {
                                splitResult = line.Split(':')[1].Trim();
                                if (!string.IsNullOrEmpty(splitResult))
                                    currentGobjName = splitResult;
                            }
                            if (line.Contains($"- id: {_filter}"))
                            {
                                string[] temp = _filesCache[i].Split('\\');
                                _sceneSearchCache.Add(new SceneSearchInfo(temp[temp.Length - 1].Split('.')[0], currentGobjName));
                                break;
                            }
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
                    temp = DrawSceneResult(_buttonRect);
                    //draw dialogue data
                    DrawDialogueResult(temp);
                    break;
                case SearchType.Filter:
                    DrawDialogueResult(temp);
                    break;
            }
        }

        private Rect DrawSceneResult(Rect rect)
        {
            float baseXPos = rect.x;
            for (int i = 0; i < _sceneSearchCache.Count; i++)
            {
                rect.width /= 2;
                rect.y += rect.height * 1.1f;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.LabelField(rect, $"Scene Name: {_sceneSearchCache[i].SceneName}");
                rect.x += rect.width * 1.1f;
                EditorGUI.LabelField(rect, $"Object Name: {_sceneSearchCache[i].ObjectName}");
                EditorGUILayout.EndHorizontal();
                rect.x = baseXPos;
                rect.y += rect.height * 1.1f;
                rect.width *= 2;
                if (GUI.Button(rect, "Go To Scene"))
                {
                    _objectNameToSelect = _sceneSearchCache[i].ObjectName;
                    EditorSceneManager.sceneOpened += HandleOnLoadScene;
                    for(int a = 0; a < _filesCache.Length; a++)
                    {
                        if (_filesCache[a].Contains(_sceneSearchCache[i].SceneName))
                        {
                            EditorSceneManager.OpenScene(_filesCache[a]);
                            break;
                        }
                    }
                }
            }
            return rect;
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

        private void HandleOnLoadScene(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            UnityEngine.Object[] temp = new UnityEngine.Object[1];
            temp[0] = GameObject.Find(_objectNameToSelect);
            Selection.objects = temp;
            EditorSceneManager.sceneOpened -= HandleOnLoadScene;
        }
    }
}
#endif