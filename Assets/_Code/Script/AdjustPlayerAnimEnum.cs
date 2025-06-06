#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Ivayami.Player;

public class AdjustPlayerAnimEnum {
    [MenuItem("Tools/Migrate Enum Values")]
    static void Migrate() {
        var scenes = AssetDatabase.FindAssets("t:Scene");
        foreach (var guid in scenes) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/_Game/Scene/Build")) continue;
            var scene = EditorSceneManager.OpenScene(path);

            var allObjs = Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var obj in allObjs) {
                var so = new SerializedObject(obj);
                var prop = so.GetIterator();
                while (prop.NextVisible(true)) {
                    if (prop.propertyType == SerializedPropertyType.Enum && prop.type == nameof(PlayerActions.InteractAnimation) && prop.enumValueIndex > 0) // new one is at 1
                    {
                        prop.enumValueIndex += 1; // so everything above 1 should be incresead 1
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
}
#endif