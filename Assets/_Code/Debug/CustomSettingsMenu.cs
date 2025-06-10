#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Ivayami.Player;
using Ivayami.Scene;
using Ivayami.Save;

namespace Ivayami.debug
{
    [InitializeOnLoad]
    public class CustomSettingsHandler
    {
        private const string _startOnCurrentScene = "startOnCurrentScene";
        public static string CurrentSceneName { get; private set; }
        public static Vector3 CameraPosition { get; private set; }
        public static bool CurrentSceneIsInBuildSettings { get; private set; }

        static CustomSettingsHandler()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeCallback;
            SceneView.duringSceneGui += HandleSceneViewGUIUpdate;
        }

        private static void HandlePlayModeCallback(PlayModeStateChange playMode)
        {
            if (playMode == PlayModeStateChange.EnteredPlayMode && EditorSceneManager.GetActiveScene().buildIndex != 0 && CustomSettingsHandler.GetEditorSettings().StartOnCurrentScene)
            {
                CurrentSceneName = EditorSceneManager.GetActiveScene().name;
                CurrentSceneIsInBuildSettings = SceneUtility.GetBuildIndexByScenePath(EditorSceneManager.GetActiveScene().path) != EditorBuildSettings.scenes.Length;
                CameraPosition = new Vector3(PlayerPrefs.GetFloat("camX"), PlayerPrefs.GetFloat("camY"), PlayerPrefs.GetFloat("camZ"));
                AsyncOperation operation = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
                if (operation != null) operation.completed += OnLoadBaseScene;
                else OnLoadBaseScene(null);
            }
        }

        public static void OnLoadBaseScene(AsyncOperation _)
        {
            if (CurrentSceneIsInBuildSettings) SceneController.Instance.OnAllSceneRequestEndDebug += OnGameplayScenesLoad;
            SaveSystem.Instance.DeleteProgress(0);
            SaveSystem.Instance.LoadProgress(0, OnSaveLoad);
        }

        private static void OnSaveLoad()
        {
            if (CurrentSceneIsInBuildSettings) SceneLoadersManager.Instance.gameObject.SetActive(true);
            else
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(CurrentSceneName, LoadSceneMode.Additive);
                if (operation != null) operation.completed += OnGameplayScenesLoad;
                else OnGameplayScenesLoad(null);
            }
            CharacterController controller = PlayerMovement.Instance.GetComponent<CharacterController>();
            controller.enabled = false;
            PlayerMovement.Instance.SetPosition(Ivayami.debug.CustomSettingsHandler.CameraPosition);
            controller.enabled = true;
        }

        public static void OnGameplayScenesLoad()
        {
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerMovement.Instance.RemoveAllBlockers();
            SceneController.Instance.OnAllSceneRequestEndDebug -= OnGameplayScenesLoad;
        }

        public static void OnGameplayScenesLoad(AsyncOperation _)
        {
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerMovement.Instance.RemoveAllBlockers();
        }

        private static void HandleSceneViewGUIUpdate(SceneView sceneView)
        {
            if (sceneView.camera.transform.position != Vector3.zero)
            {
                PlayerPrefs.SetFloat("camX", sceneView.camera.transform.position.x);
                PlayerPrefs.SetFloat("camY", sceneView.camera.transform.position.y);
                PlayerPrefs.SetFloat("camZ", sceneView.camera.transform.position.z);
            }
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