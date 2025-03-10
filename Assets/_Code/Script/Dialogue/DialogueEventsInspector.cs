#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ivayami.Dialogue
{
    [CustomEditor(typeof(DialogueEvents))]
    public class DialogueEventsInspector : Editor
    {
        SerializedProperty /*debugLogs,*/ events;
        private static Dialogue[] _dialogueAssets;
        private Dictionary<int, string> _eventsDic = new Dictionary<int, string>();
        private int _previousSize;
        private int _currentEventIndex;
        public override void OnInspectorGUI()
        {
            if (_dialogueAssets == null) UpdateDialoguesList();

            //EditorGUILayout.PropertyField(debugLogs, new GUIContent("Debug Log"));
            EditorGUILayout.PropertyField(events, new GUIContent("Events"));
            
            serializedObject.ApplyModifiedProperties();
            HandleEventsValuesUpdate();
            _previousSize = events.arraySize;
        }

        private void OnEnable()
        {
            //debugLogs = serializedObject.FindProperty("_debugLogs");
            events = serializedObject.FindProperty("_events");
            DialogueEvents instance = (DialogueEvents)target;
            if (instance.Events != null)
            {
                for (int i = 0; i < instance.Events.Length; i++)
                {
                    _currentEventIndex++;
                    instance.Events[i].InternalId = _currentEventIndex;
                    _eventsDic.Add(_currentEventIndex, instance.Events[i].id);
                }
            }
            _previousSize = instance.Events.Length;
        }

        private void OnDestroy()
        {
            if (target == null)
            {
                HandleComponentDestroy();
            }
        }

        private void HandleEventsValuesUpdate()
        {
            //removed
            if (_previousSize > events.arraySize)
            {
                int i;
                List<int> keys = _eventsDic.Keys.ToList();
                List<int> exists = new List<int>();
                for (i = 0; i < events.arraySize; i++)
                {
                    if (keys.Contains(events.GetArrayElementAtIndex(i).FindPropertyRelative("InternalId").intValue))
                    {
                        exists.Add(events.GetArrayElementAtIndex(i).FindPropertyRelative("InternalId").intValue);
                    }
                }
                for (i = 0; i < exists.Count; i++)
                {
                    keys.Remove(exists[i]);
                }
                for (i = 0; i < keys.Count; i++)
                {
                    RemoveEventIDFromDialogeOrSpeech(_eventsDic[keys[i]]);
                    _eventsDic.Remove(keys[i]);
                }
            }
            //added
            else if (_previousSize < events.arraySize)
            {
                DialogueEvents instance = (DialogueEvents)target;
                List<int> indexsFound = new List<int>();
                for (int i = 0; i < instance.Events.Length; i++)
                {
                    if (!indexsFound.Contains(instance.Events[i].InternalId))
                    {
                        indexsFound.Add(instance.Events[i].InternalId);                        
                    }
                    else
                    {
                        _currentEventIndex++;
                        instance.Events[i] = new SpeechEvent(_currentEventIndex);
                        _eventsDic.Add(instance.Events[i].InternalId, instance.Events[i].id);
                    }
                }
            }
            //update values
            else
            {
                SerializedProperty prop;
                for (int i = 0; i < events.arraySize; i++)
                {
                    prop = events.GetArrayElementAtIndex(i);
                    if (_eventsDic[prop.FindPropertyRelative("InternalId").intValue] != prop.FindPropertyRelative("id").stringValue)
                    {
                        UpdateEventIDFromDialogeOrSpeech(_eventsDic[prop.FindPropertyRelative("InternalId").intValue], prop.FindPropertyRelative("id").stringValue);
                        _eventsDic[prop.FindPropertyRelative("InternalId").intValue] = prop.FindPropertyRelative("id").stringValue;
                    }
                }
            }
        }

        public static void UpdateDialoguesList()
        {
            _dialogueAssets = Resources.LoadAll<Dialogue>("Dialogues");
        }

        //assumes that there will be no duplicate ID in the Dialoge Assets
        private void RemoveEventIDFromDialogeOrSpeech(string eventID)
        {
            bool objectRecorded;
            for (int i = 0; i < _dialogueAssets.Length; i++)
            {
                objectRecorded = false;
                if (_dialogueAssets[i].onEndEventId == eventID)
                {
                    if (!objectRecorded) Undo.RecordObject(_dialogueAssets[i], "EndEventId");
                    objectRecorded = true;
                    _dialogueAssets[i].onEndEventId = null;
                    EditorUtility.SetDirty(_dialogueAssets[i]);
                    //return;
                }
                for (int a = 0; a < _dialogueAssets[i].dialogue.Length; a++)
                {
                    if (_dialogueAssets[i].dialogue[a].EventId == eventID)
                    {
                        if (!objectRecorded) Undo.RecordObject(_dialogueAssets[i], "SpeechEventId");
                        objectRecorded = true;
                        _dialogueAssets[i].dialogue[a].EventId = null;
                        EditorUtility.SetDirty(_dialogueAssets[i]);
                        //return;
                    }
                }
            }
        }

        private void UpdateEventIDFromDialogeOrSpeech(string previousID, string currentID)
        {
            bool objectRecorded;
            for (int i = 0; i < _dialogueAssets.Length; i++)
            {
                objectRecorded = false;
                if (_dialogueAssets[i].onEndEventId == previousID && !string.IsNullOrEmpty(_dialogueAssets[i].onEndEventId))
                {
                    if (!objectRecorded) Undo.RecordObject(_dialogueAssets[i], "EndEventId");
                    objectRecorded = true;
                    _dialogueAssets[i].onEndEventId = currentID;
                    EditorUtility.SetDirty(_dialogueAssets[i]);
                    //return;
                }
                for (int a = 0; a < _dialogueAssets[i].dialogue.Length; a++)
                {
                    if (_dialogueAssets[i].dialogue[a].EventId == previousID && !string.IsNullOrEmpty(_dialogueAssets[i].dialogue[a].EventId))
                    {
                        if (!objectRecorded) Undo.RecordObject(_dialogueAssets[i], "SpeechEventId");
                        objectRecorded = true;
                        _dialogueAssets[i].dialogue[a].EventId = currentID;
                        EditorUtility.SetDirty(_dialogueAssets[i]);
                        //return;
                    }
                }
            }
        }

        private void HandleComponentDestroy()
        {
            string[] temp = _eventsDic.Values.ToArray();
            for (int i = 0; i < temp.Length; i++)
            {
                RemoveEventIDFromDialogeOrSpeech(temp[i]);
            }
            _eventsDic.Clear();
        }

    }
}
#endif