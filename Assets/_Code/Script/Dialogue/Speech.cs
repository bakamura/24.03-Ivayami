#if UNITY_EDITOR
using UnityEngine;

namespace Ivayami.Dialogue
{
    [System.Serializable]
    public struct Speech
    {
        [ReadOnly] public string Language;
        [TextArea(1, 50)] public string content;
    }
}
#endif