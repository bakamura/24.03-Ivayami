using UnityEngine;

namespace Paranapiacaba.Dialogue {
    [System.Serializable]
    public class Speech {

        public string announcerName;
        [TextArea(1, 50)] public string content;
        public string eventId;

    }
}