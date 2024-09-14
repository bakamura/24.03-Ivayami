#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Ivayami.Save
{
    internal static class SaveObjectsUtilityWindow
    {
        [MenuItem("Ivayami/Save/Assign Unique Keys...", false, 0)]
        public static void AssignUniqueKeysDialog()
        {
            if (EditorUtility.DisplayDialog("Assign Unique SaveObject Keys", "Assign unique keys to all SaveObject components in the current loaded scenes?", "OK", "Cancel"))
            {
                AssignUniqueKeysInScene();
            }
        }

        private static void AssignUniqueKeysInScene()
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var s = EditorSceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {                    
                    var allGameObjects = s.GetRootGameObjects();
                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        AssignUniqueKeysInTransformHierarchy(allGameObjects[j].transform, s.name);
                    }
                }
            }
        }

        private static void AssignUniqueKeysInTransformHierarchy(Transform t, string sceneName)
        {
            if (t == null) return;
            var savers = t.GetComponentsInChildren<SaveObject>();
            for (int i = 0; i < savers.Length; i++)
            {
                Undo.RecordObject(savers[i], "Key");
                savers[i].ID = $"{savers[i].gameObject.name}_{sceneName}_{Mathf.Abs(savers[i].GetInstanceID())}";
            }
        }
    }
}
#endif