using UnityEngine;
using System.Collections.Generic;

namespace Paranapiacaba.Dialogue {
    public class DialogueEvents : MonoBehaviour {
        [SerializeField] private bool _debugLogs;
        [SerializeField] private SpeechEvent[] _events;
        private Dictionary<string, SpeechEvent> _eventsDictionary = new Dictionary<string, SpeechEvent>();
        public SpeechEvent[] Events => _events;

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
                    if (_debugLogs)
                    {
                        Debug.LogWarning($"the Dialogue Event ID {_events[i].id} is already in use");
                    }
                }
            }

            DialogueController.Instance.SetCurrentDialogueEvents(this);
        }

        public void TriggerEvent(string eventId) {
            if (_eventsDictionary.ContainsKey(eventId))
            {
                _eventsDictionary[eventId].unityEvent?.Invoke();
            }
        }       

    }
}