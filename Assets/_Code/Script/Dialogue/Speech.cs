using UnityEngine;

namespace Ivayami.Dialogue {
    [System.Serializable]
    public class Speech {

        public string announcerName;
        public string eventId;
#if UNITY_EDITOR
        public string FilterTags;
#endif
        [Min(0f), Tooltip("Wait for this time to continue dialogue. Will not continue if a cutscene is playing or value is 0")] public float FixedDurationInSpeech;
        [TextArea(1, 50)] public string content;

    }
}