using UnityEngine;

namespace Ivayami.Dialogue
{
    [System.Serializable]
    public struct Speech
    {
        [EnumToString(3)] public LanguageTypes LanguageType;
        public string announcerName;
        [TextArea(1, 50)] public string content;
    }
}