using UnityEngine;

namespace Ivayami.Dialogue {
    [System.Serializable]
    public class Speech {

        public string announcerName;
        public string eventId;
        [TextArea(1, 50)] public string content;

    }
}