using UnityEditor;
using UnityEngine;
using System;

namespace Ivayami.Dialogue
{
	[CustomPropertyDrawer(typeof(EnumToStringAttribute))]
	internal sealed class EnumToStringAttributeDrawer : PropertyDrawer
	{
		//public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		//{
		//	return property.propertyType == SerializedPropertyType.Enum
		//			   ? EditorGUIUtility.singleLineHeight
		//			   : EditorGUIUtility.singleLineHeight * 2f;
		//}					

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
			for(int i = 0; i < instance.IndentLevel; i++)
            {
				EditorGUI.indentLevel++;
            }
			//position = GUILayoutUtility.GetLastRect();
			EditorGUI.LabelField(position, Enum.GetNames(typeof(LanguageTypes))[index]);
			for (int i = 0; i < instance.IndentLevel; i++)
			{
				EditorGUI.indentLevel--;
			}
		}

		private void DrawPropertyWithWarning(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = base.GetPropertyHeight(property, label);
			EditorGUI.HelpBox(position, "Language Attribute is only valid for the Enum type", MessageType.Warning);

			position.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, property, label);
		}
	}
}