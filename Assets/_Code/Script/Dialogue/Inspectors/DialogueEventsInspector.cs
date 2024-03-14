#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Paranapiacaba.Dialogue
{
    [CustomEditor(typeof(DialogueEvents))]
    public class DialogueEventsInspector : Editor
    {
        SerializedProperty debugLogs, events;
        private static Dialogue[] _dialogueAssets;
        private List<string> _previousIDs = new List<string>();
        private SpeechEvent[] _previousEventArray;
        public override void OnInspectorGUI()
        {
            if (_dialogueAssets == null) UpdateDialoguesList();

            EditorGUILayout.PropertyField(debugLogs, new GUIContent("Debug Log"));
            EditorGUILayout.PropertyField(events, new GUIContent("Events"));

            HandleEventsValuesUpdate(false);
            serializedObject.ApplyModifiedProperties();
            HandleEventsValuesUpdate(true);
        }

        private void HandleEventsValuesUpdate(bool updateCurrentValues)
        {
            DialogueEvents instance = (DialogueEvents)target;
            SpeechEvent[] dialogArray = instance.Events;
            if (!updateCurrentValues)
            {
                _previousIDs.Clear();
                for (int i = 0; i < dialogArray.Length; i++)
                {
                    if (!_previousIDs.Contains(dialogArray[i].id)) _previousIDs.Add(dialogArray[i].id);
                }
                _previousEventArray = new SpeechEvent[dialogArray.Length];
                Array.Copy(dialogArray, _previousEventArray, dialogArray.Length);
            }
            else
            {
                int arrayLenght = _previousIDs.Count;
                if (dialogArray.Length < _previousEventArray.Length)
                {
                    arrayLenght = dialogArray.Length;
                    List<SpeechEvent> temp = _previousEventArray.ToList();
                    for (int i = 0; i < dialogArray.Length; i++)
                    {
                        //talvez n funciona pq ele pega uma copia
                        temp.Remove(dialogArray[i]);
                    }
                    for (int i = 0; i < temp.Count; i++)
                    {
                        RemoveEventIDFromDialogeOrSpeech(temp[i].id);
                    }
                }                
                for (int i = 0; i < arrayLenght; i++)
                {
                    if (_previousIDs[i] != dialogArray[i].id)
                    {
                        //Debug.Log($"previous {_previousIDs[i]}, current {dialogArray[i].id}");
                        UpdateEventIDFromDialogeOrSpeech(_previousIDs[i], dialogArray[i].id);
                    }
                }
            }
        }

        private void OnEnable()
        {
            debugLogs = serializedObject.FindProperty("_debugLogs");
            events = serializedObject.FindProperty("_events");
        }

        public static void UpdateDialoguesList()
        {
            _dialogueAssets = Resources.LoadAll<Dialogue>("Dialogues");
        }

        //assumes that there will be no duplicate ID in the Dialoge Assets
        private void RemoveEventIDFromDialogeOrSpeech(string eventID)
        {
            for (int i = 0; i < _dialogueAssets.Length; i++)
            {
                if (_dialogueAssets[i].onEndEventId == eventID)
                {
                    _dialogueAssets[i].onEndEventId = null;
                    return;
                }
                for (int a = 0; a < _dialogueAssets[i].dialogue.Length; a++)
                {
                    if (_dialogueAssets[i].dialogue[a].eventId == eventID)
                    {
                        _dialogueAssets[i].dialogue[a].eventId = null;
                        return;
                    }
                }
            }
        }

        private void UpdateEventIDFromDialogeOrSpeech(string previousID, string currentID)
        {
            for (int i = 0; i < _dialogueAssets.Length; i++)
            {
                if (_dialogueAssets[i].onEndEventId == previousID && !string.IsNullOrEmpty(_dialogueAssets[i].onEndEventId))
                {
                    _dialogueAssets[i].onEndEventId = currentID;
                    return;
                }
                for (int a = 0; a < _dialogueAssets[i].dialogue.Length; a++)
                {
                    if (_dialogueAssets[i].dialogue[a].eventId == previousID && !string.IsNullOrEmpty(_dialogueAssets[i].dialogue[a].eventId))
                    {
                        _dialogueAssets[i].dialogue[a].eventId = currentID;
                        return;
                    }
                }
            }
        }
    }
}
#endif