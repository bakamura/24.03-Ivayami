using UnityEngine;

namespace Ivayami.Dialogue {
    [System.Serializable]
    public class Speech {

        public string announcerName;
        public string eventId;
        [TextArea(1, 50)] public string content;
#if UNITY_EDITOR
        [SerializeField] private string _filterTags;
        public string FilterTags => _filterTags;
#endif

    }
}