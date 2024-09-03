#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ivayami.Scene
{
    [CustomPropertyDrawer(typeof(SceneDropdownAttribute))]	
    internal sealed class SceneDropdownAttributeDrawer : PropertyDrawer
    {
		private bool _isInitialized;
		private readonly List<string> _sceneNames = new();

		private void Initialize()
		{
			_isInitialized = true;

			_sceneNames.Clear();
			_sceneNames.Add("None");

			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
				string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

				if (!_sceneNames.Contains(sceneName))
					_sceneNames.Add(sceneName);
			}

			_sceneNames.Remove("Main");
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return property.propertyType == SerializedPropertyType.String
					   ? EditorGUIUtility.singleLineHeight
					   : EditorGUIUtility.singleLineHeight * 2f;
		}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_isInitialized)
                Initialize();

            if (property.propertyType == SerializedPropertyType.String)
                DrawSceneDropdown(position, property, label);
            else
                DrawPropertyWithWarning(position, property, label);
        }

        private void DrawSceneDropdown(Rect position, SerializedProperty property, GUIContent label)
		{
			string value = property.stringValue;
			int selectedIndex = string.IsNullOrEmpty(value) ? 0 : _sceneNames.IndexOf(value);

			EditorGUI.BeginChangeCheck();

			selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, _sceneNames.ToArray());

			if (EditorGUI.EndChangeCheck())
				property.stringValue = selectedIndex == 0 ? string.Empty : _sceneNames.ElementAtOrDefault(selectedIndex);
		}

		private void DrawPropertyWithWarning(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = base.GetPropertyHeight(property, label);
			EditorGUI.HelpBox(position, "Scene Dropdown Attribute is only valid for strings", MessageType.Warning);

			position.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, property, label);
		}
	}
}
#endif