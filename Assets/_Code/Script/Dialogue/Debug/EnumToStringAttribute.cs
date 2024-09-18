using UnityEngine;

namespace Ivayami.Dialogue
{
    public sealed class EnumToStringAttribute : PropertyAttribute
    {
        public readonly int IndentLevel;
        public EnumToStringAttribute(int indentLevel = 0)
        {
            IndentLevel = indentLevel;
        }
    }
}