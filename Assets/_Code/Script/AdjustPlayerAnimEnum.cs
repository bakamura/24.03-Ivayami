#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Ivayami.Player;

namespace Ivayami.debug {
    public class AdjustPlayerAnimEnum {

        //[MenuItem("Tools/Migrate Enum Values")]
        static void Migrate() {
            var scenes = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in scenes) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/_Game/Scene/Build")) continue;
                var scene = EditorSceneManager.OpenScene(path);

                var allObjs = Object.FindObjectsOfType<MonoBehaviour>(true);
                foreach (var obj in allObjs) {
                    var so = new SerializedObject((Object)obj);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true)) {
                        var type = obj.GetType();
                        var field = type.GetField(prop.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (field != null && field.FieldType == typeof(PlayerActions.InteractAnimation) && prop.enumValueIndex > 0) {
                            Debug.Log($"{prop.name}");
                            prop.enumValueIndex += 1;
                            so.ApplyModifiedProperties();
                        }
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Enum migration complete.");
        }

        //[MenuItem("Tools/Show all InteractAnimation")]
        static void ShowInteractAnimation() {
            var allObjs = Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var obj in allObjs) {
                var so = new SerializedObject((Object)obj);
                var prop = so.GetIterator();
                bool shouldDisplayName = false;
                while (prop.NextVisible(true)) {
                    var type = obj.GetType();
                    var field = type.GetField(prop.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null && field.FieldType == typeof(PlayerActions.InteractAnimation) && prop.enumValueIndex > 0) shouldDisplayName = true;
                }
                if (shouldDisplayName) Debug.LogFormat(obj.name);
            }
            Debug.Log("A");
        }
    }

}
#endif
