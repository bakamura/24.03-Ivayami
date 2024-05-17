using UnityEngine;

namespace Ivayami.Dialogue {
    [System.Serializable]
    public class Speech {

        public string announcerName;
        public string eventId;
#if UNITY_EDITOR
        [SerializeField] private string _filterTags;
        public string FilterTags => _filterTags;
#endif
        [Min(0f), Tooltip("will force to wait this time to continue the dialogue, only applies when lock player in dialogue")] public float FixedDurationInSpeech;
        [TextArea(1, 50)] public string content;

    }
}