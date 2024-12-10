using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(EnumToStringAttribute))]
internal sealed class EnumToStringAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Enum)
            DrawEnumAsString(position, property, label);
        else
            DrawPropertyWithWarning(position, property, label);
    }

    private void DrawEnumAsString(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumToStringAttribute instance = (EnumToStringAttribute)attribute;
        int index = property.enumValueIndex;
        for (int i = 0; i < instance.IndentLevel; i++)
        {
            EditorGUI.indentLevel++;
        }
        //position = GUILayoutUtility.GetLastRect();
        EditorGUI.LabelField(position, Enum.GetNames(instance.EnumType)[index]);
        for (int i = 0; i < instance.IndentLevel; i++)
        {
            EditorGUI.indentLevel--;
        }
    }

    private void DrawPropertyWithWarning(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = base.GetPropertyHeight(property, label);
        EditorGUI.HelpBox(position, "This attribute is only valid for Enum types", MessageType.Warning);

        position.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, property, label);
    }
}