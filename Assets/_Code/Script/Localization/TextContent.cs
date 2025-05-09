using UnityEngine;

namespace Ivayami.Localization
{
    [System.Serializable]
    public struct TextContent
    {
        [ReadOnly] public string Language;
        public string Name;
        [TextArea] public string Description;
    }
}