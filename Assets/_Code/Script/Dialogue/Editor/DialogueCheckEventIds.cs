using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Ivayami.Dialogue
{
    internal static class DialogueCheckEventIds
    {
        [System.Serializable]
        private struct EventDataResult
        {
            public string Message;
            public bool Repeated;

            public EventDataResult(string message, bool repeated)
            {
                Message = message;
                Repeated = repeated;
            }
        }

        private static List<UnityEngine.SceneManagement.Scene> _scenesToCloseAfterProcess = new List<UnityEngine.SceneManagement.Scene>();

        [MenuItem("Ivayami/DialogueUtilities/Check For Duplicate Event IDs", false, 0)]
        public static void CheckForDuplicateEventIDs()
        {
            if (EditorUtility.DisplayDialog("Check For Duplicate Event IDs", "Check all scenes in the build settings for duplicated dialogue event IDs? WARNING THIS PROCESS IS HEAVY ON THE PC", "OK", "Cancel"))
            {
                OpenAllScenes();
            }
        }

        private static void OpenAllScenes()
        {
            UnityEngine.SceneManagement.Scene scene;            
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scene = EditorSceneManager.GetSceneByPath(EditorBuildSettings.scenes[i].path);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    _scenesToCloseAfterProcess.Add(EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path, OpenSceneMode.Additive));
                }
            }
            CheckForDuplicatedIDs();
        }

        private static void CheckForDuplicatedIDs()
        {
            //Debug.Log("Start Checking");
            //UnityEngine.SceneManagement.Scene scene;
            //GameObject[] gobjs;
            Dictionary<string, EventDataResult> eventsDuplicates = new Dictionary<string, EventDataResult>();
            //DialogueEvents dialogueEvents;
            DialogueEvents[] events = Object.FindObjectsByType<DialogueEvents>(FindObjectsSortMode.None);

            for(int i =0; i < events.Length; i++)
            {
                for (int a = 0; a < events[i].Events.Length; a++)
                {
                    if (eventsDuplicates.ContainsKey(events[i].Events[a].id))
                        eventsDuplicates[events[i].Events[a].id] = new EventDataResult(eventsDuplicates[events[i].Events[a].id].Message + $"the object {events[i].name} at scene {GameObject.GetScene(events[i].gameObject.GetInstanceID()).name} at the element of {a} in the event list, ", true);
                    else
                        eventsDuplicates.Add(events[i].Events[a].id, new EventDataResult($"the object {events[i].name} at scene {GameObject.GetScene(events[i].gameObject.GetInstanceID()).name} at the element of {a} in the event list, ", false));
                }
            }
            bool foundDuplicates = false;
            foreach(string id in eventsDuplicates.Keys)
            {
                if (eventsDuplicates[id].Repeated)
                {
                    foundDuplicates = true;
                    Debug.Log($"The event {id} has been repeated in: {eventsDuplicates[id].Message}");
                }
            }
            if (!foundDuplicates) Debug.Log("No duplicated Dialogue Events found");
            for(int i = 0; i < _scenesToCloseAfterProcess.Count; i++)
            {
                EditorSceneManager.CloseScene(_scenesToCloseAfterProcess[i], true);
            }
            _scenesToCloseAfterProcess.Clear();
            //for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            //{
            //    scene = EditorSceneManager.GetSceneAt(i);
            //    gobjs = scene.GetRootGameObjects();
            //    for(int a = 0; a < gobjs.Length; a++)
            //    {
            //        dialogueEvents = gobjs[a].transform.GetComponentInChildren<DialogueEvents>();
            //        if (dialogueEvents)
            //        {
            //            for(int b = 0; b < dialogueEvents.Events.Length; b++)
            //            {
            //                if (eventsDuplicates.ContainsKey(dialogueEvents.Events[b].id))
            //                    eventsDuplicates[dialogueEvents.Events[b].id] += $"the object {dialogueEvents.name} at scene {scene.name} at the element of {b} in the event list, ";
            //                else
            //                    eventsDuplicates.Add(dialogueEvents.Events[b].id, $"the object {dialogueEvents.name} at scene {scene.name} at the element of {b} in the event list, ");
            //            }
            //        }
            //    }
            //}
        }
    }
}