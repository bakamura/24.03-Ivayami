using UnityEngine;

public sealed class EnumToStringAttribute : PropertyAttribute
{
    public readonly int IndentLevel;
    public readonly System.Type EnumType;
    public EnumToStringAttribute(System.Type enumType, int indentLevel = 0)
    {
        IndentLevel = indentLevel;
        EnumType = enumType;
    }
}