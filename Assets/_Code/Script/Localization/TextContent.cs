namespace Ivayami.Localization
{
    [System.Serializable]
    public struct TextContent
    {
        [ReadOnly] public string Language;
        public string Name;
        public string Description;
    }
}