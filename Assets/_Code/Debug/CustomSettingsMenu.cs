#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Ivayami.debug
{
    [InitializeOnLoad]
    public class CustomSettingsHandler
    {
        private const string _startOnCurrentScene = "startOnCurrentScene";
        public static string CurrentSceneName { get; private set; }
        public static Vector3 CurrentCameraPosition { get; private set; }

        static CustomSettingsHandler()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeCallback;
            SceneView.duringSceneGui += HandleSceneGUI;
        }

        private static void HandlePlayModeCallback(PlayModeStateChange playMode)
        {
            if (playMode == PlayModeStateChange.EnteredPlayMode && EditorSceneManager.GetActiveScene().buildIndex != 0 && CustomSettingsHandler.GetEditorSettings().StartOnCurrentScene)
            {
                CurrentSceneName = EditorSceneManager.GetActiveScene().name;
                SceneManager.LoadScene(0);
            }
        }

        private static void HandleSceneGUI(SceneView view)
        {
            CurrentCameraPosition = view.camera.transform.position;
        }

        public class NewCustomSettings
        {
            public bool StartOnCurrentScene;
        }

        public static NewCustomSettings GetEditorSettings()
        {
            return new NewCustomSettings
            {
                StartOnCurrentScene = EditorPrefs.GetBool(_startOnCurrentScene, false),
            };
        }

        public static void SetEditorSettings(NewCustomSettings settings)
        {
            EditorPrefs.SetBool(_startOnCurrentScene, settings.StartOnCurrentScene);
        }
    }

    internal class SettingsGUIContent
    {
        private static GUIContent enableCustomBool1 = new GUIContent("Start Game On Current Scene", "");

        public static void DrawSettingsButtons(CustomSettingsHandler.NewCustomSettings settings)
        {
            EditorGUI.indentLevel += 1;

            settings.StartOnCurrentScene = EditorGUILayout.ToggleLeft(enableCustomBool1, settings.StartOnCurrentScene);

            EditorGUI.indentLevel -= 1;
        }
    }
#if UNITY_2018_3_OR_NEWER
    static class CustomSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Ivayami", SettingsScope.User)
            {
                label = "Ivayami",

                guiHandler = (searchContext) =>
                {
                    CustomSettingsHandler.NewCustomSettings settings = CustomSettingsHandler.GetEditorSettings();

                    EditorGUI.BeginChangeCheck();
                    SettingsGUIContent.DrawSettingsButtons(settings);

                    if (EditorGUI.EndChangeCheck())
                    {
                        CustomSettingsHandler.SetEditorSettings(settings);
                    }

                },

                // Keywords for the search bar in the Unity Preferences menu
                keywords = new HashSet<string>(new[] { "Ivayami", "Custom", "Settings" })
            };

            return provider;
        }
    }
#endif
}
#endif