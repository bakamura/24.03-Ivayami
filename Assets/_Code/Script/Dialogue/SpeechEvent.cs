using UnityEngine.Events;
using System;
using UnityEngine;

namespace Ivayami.Dialogue
{
    [Serializable]
    public class SpeechEvent
    {
        public string id;
#if UNITY_EDITOR
        [HideInInspector] public int InternalId;
#endif
        public UnityEvent unityEvent;
#if UNITY_EDITOR
        public SpeechEvent(int internalId)
        {
            InternalId = internalId;
            id = null;
            unityEvent = new UnityEvent();
        }
#endif
    }
}