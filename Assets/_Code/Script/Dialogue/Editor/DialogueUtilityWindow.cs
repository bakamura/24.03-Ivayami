#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System;
using System.Linq;

namespace Ivayami.Dialogue
{
    internal sealed class DialogueUtilityWindow : EditorWindow
    {
        private Dialogue[] _allDialogues;
        private string _filter;
        private Rect _buttonRect;
        private List<DialogueSearchInfo> _dialoguesSearchCache = new List<DialogueSearchInfo>();
        private Dictionary<string, List<SceneSearchInfo>> _sceneSearchCache = new Dictionary<string, List<SceneSearchInfo>>();
        private SearchType _currentSearchType;
        private string _objectNameToSelect;
        private string[] _filesCache;
        private SearchType _previousSearchType;
        private int _currentDropdownIndex;
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
            public string EventId;

            public DialogueSearchInfo(Dialogue dialogue, sbyte speechIndex, string eventId)
            {
                Dialogue = dialogue;
                SpeechIndex = speechIndex;
                EventId = eventId;
            }
        }

        [Serializable]
        private struct SceneSearchInfo
        {
            public string EventId;
            public string ObjectName;
            public string SceneName;

            public SceneSearchInfo(string eventId, string objectName, string sceneName)
            {
                EventId = eventId;
                ObjectName = objectName;
                SceneName = sceneName;
            }
        }

        [MenuItem("Ivayami/DialogueUtilities/UtilityWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<DialogueUtilityWindow>();
            window.titleContent = new GUIContent("Dialogue Utilities");
            window.Show();
        }

        private void OnGUI()
        {
            _previousSearchType = _currentSearchType;
            _currentSearchType = (SearchType)EditorGUILayout.EnumPopup("Search Type", _currentSearchType);
            _filter = EditorGUILayout.TextField("Search", _filter);

            _buttonRect = GUILayoutUtility.GetLastRect();
            _buttonRect.y += _buttonRect.height * 1.2f;
            if (!string.IsNullOrEmpty(_filter) && GUI.Button(_buttonRect, "Search"))
            {
                UpdateSearchResult();
            }
            else if (string.IsNullOrEmpty(_filter) || _previousSearchType != _currentSearchType)
            {
                _dialoguesSearchCache.Clear();
                _sceneSearchCache.Clear();
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
                    string sceneName;
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
                            //if (line.Contains("- id:")) 
                            //    Debug.Log(line);
                            if (line.Contains($"- id: ") && line.ToUpper().Contains(_filter.ToUpper()))
                            {
                                sceneName = _filesCache[i].Split('\\')[^1].Split('.')[0];
                                if (_sceneSearchCache.ContainsKey(sceneName))
                                {
                                    _sceneSearchCache[sceneName].Add(new SceneSearchInfo(line.Split(':')[1].Trim(' '), currentGobjName, sceneName));
                                }
                                else
                                {
                                    _sceneSearchCache.Add(sceneName, new List<SceneSearchInfo>() { new SceneSearchInfo(line.Split(':')[1].Trim(' '), currentGobjName, sceneName) });
                                }
                                //break;
                            }
                        }
                    }
                    //collect dialogue assets with the events
                    for (int i = 0; i < _allDialogues.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(_allDialogues[i].onEndEventId) && _allDialogues[i].onEndEventId.ToUpper().Contains(_filter.ToUpper()))
                        {
                            _dialoguesSearchCache.Add(new DialogueSearchInfo(_allDialogues[i], -1, _allDialogues[i].onEndEventId));
                        }
                        for (int a = 0; a < _allDialogues[i].dialogue.Length; a++)
                        {
                            if (!string.IsNullOrEmpty(_allDialogues[i].dialogue[a].EventId) && _allDialogues[i].dialogue[a].EventId.ToUpper().Contains(_filter.ToUpper()))
                            {
                                _dialoguesSearchCache.Add(new DialogueSearchInfo(_allDialogues[i], (sbyte)a, _allDialogues[i].dialogue[a].EventId));
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
                                _dialoguesSearchCache.Add(new DialogueSearchInfo(_allDialogues[i], (sbyte)a, null));
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
                    string[] keys = _sceneSearchCache.Keys.ToArray();
                    DialogueSearchInfo info;
                    SceneSearchInfo[] differentObjectsFound;
                    for (int i = 0; i < _sceneSearchCache.Keys.Count; i++)
                    {
                        //draw dialogue data
                        for (int a = 0; a < _sceneSearchCache[keys[i]].Count; a++)
                        {
                            //info = new DialogueSearchInfo();
                            //for(int b = 0; b < _dialoguesSearchCache.Count; b++)
                            //{
                            //    if (_dialoguesSearchCache[b].EventId.Contains(_sceneSearchCache[keys[i]][a].EventId))
                            //    {
                            //        info = _dialoguesSearchCache[b];
                            //        break;
                            //    }
                            //}
                            info = _dialoguesSearchCache.Where(x => x.EventId.Contains(_sceneSearchCache[keys[i]][a].EventId)).FirstOrDefault();
                            if (info.Dialogue)
                            {
                                temp = DrawDialogueResult(temp, info);
                            }
                        }
                        //draw scene data
                        differentObjectsFound = _sceneSearchCache[keys[i]].Where(x => x.EventId.Contains(_sceneSearchCache[keys[i]][0].EventId)).ToArray();
                        //draw all objects inside scene that contais the event id
                        for (int a = 0; a < differentObjectsFound.Length; a++)
                        {
                            temp = DrawSceneResult(temp, differentObjectsFound[a]);
                        }
                        //draw the Go to scene btn and autoSelectObject dropdown
                        temp = DrawSceneButton(temp, _sceneSearchCache[keys[i]]);
                    }
                    break;
                case SearchType.Filter:
                    for (int i = 0; i < _dialoguesSearchCache.Count; i++)
                    {
                        temp = DrawDialogueResult(temp, _dialoguesSearchCache[i]);
                    }
                    break;
            }
        }

        private Rect DrawSceneResult(Rect rect, SceneSearchInfo info)
        {
            rect.width /= 2;
            rect.y += rect.height * 1.1f;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(rect, $"Scene Name: {info.SceneName}");
            rect.x += rect.width * 1.1f;
            EditorGUI.LabelField(rect, $"Object Name: {info.ObjectName}");
            EditorGUILayout.EndHorizontal();
            return new Rect(_buttonRect.x, rect.y, _buttonRect.width, _buttonRect.height);
        }

        private Rect DrawSceneButton(Rect rect, List<SceneSearchInfo> infos)
        {
            rect.y += rect.height * 1.1f;
            EditorGUI.LabelField(rect, "Object To Select On Open Scene");
            rect.y += rect.height * 1.1f;
            _currentDropdownIndex = EditorGUI.Popup(rect, _currentDropdownIndex, infos.Select(x => x.ObjectName).ToArray());
            rect.y += rect.height * 1.1f;
            if (GUI.Button(rect, "Go To Scene"))
            {
                _objectNameToSelect = infos[_currentDropdownIndex].ObjectName;
                EditorSceneManager.sceneOpened += HandleOnLoadScene;
                for (int a = 0; a < _filesCache.Length; a++)
                {
                    if (_filesCache[a].Contains(infos[0].SceneName))
                    {
                        EditorSceneManager.OpenScene(_filesCache[a]);
                        break;
                    }
                }
            }
            return new Rect(_buttonRect.x, rect.y, _buttonRect.width, _buttonRect.height);
        }

        private Rect DrawDialogueResult(Rect rect, DialogueSearchInfo info)
        {
            float baseXPos = rect.x;
            rect.width /= 2;
            //for (int i = 0; i < _dialoguesSearchCache.Count; i++)
            //{
            rect.y += rect.height * 1.1f;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.ObjectField(rect, info.Dialogue, typeof(Dialogue), false);
            rect.x += rect.width * 1.1f;
            if (info.SpeechIndex == -1) EditorGUI.LabelField(rect, "On End Event");
            else EditorGUI.LabelField(rect, $"Speech Index: {info.SpeechIndex}");
            EditorGUILayout.EndHorizontal();
            rect.x = baseXPos;
            //}
            return new Rect(_buttonRect.x, rect.y, _buttonRect.width, _buttonRect.height);
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