using UnityEngine;
using System.Collections.Generic;

namespace Ivayami.Dialogue
{
    public class DialogueEvents : MonoBehaviour
    {
        //[SerializeField] private bool _debugLogs;
        [SerializeField] private SpeechEvent[] _events;
        private Dictionary<string, SpeechEvent> _eventsDictionary = new Dictionary<string, SpeechEvent>();
#if UNITY_EDITOR
        public SpeechEvent[] Events => _events;
#endif

        private void Start()
        {
            for (int i = 0; i < _events.Length; i++)
            {
                if (!_eventsDictionary.ContainsKey(_events[i].id))
                {
                    _eventsDictionary.Add(_events[i].id, _events[i]);
                }
                else
                {
                    //if (_debugLogs)
                    //{
                    Debug.LogWarning($"the Dialogue Event ID {_events[i].id} from the DialogueEvent {name} at the element {i} is already in use, please change the ID");
                    //}
                }
            }
        }

        private void OnEnable()
        {
            if (DialogueController.Instance) DialogueController.Instance.UpdateDialogueEventsList(this);
        }

        private void OnDisable()
        {
            if (DialogueController.Instance) DialogueController.Instance.UpdateDialogueEventsList(this);
        }

        public bool TriggerEvent(string eventId)
        {
            if (CheckForEvent(eventId))
            {
                _eventsDictionary[eventId].unityEvent?.Invoke();
                return true;
            }
            return false;
        }

        public bool CheckForEvent(string eventId)
        {
            return _eventsDictionary.ContainsKey(eventId);
        }

    }
}