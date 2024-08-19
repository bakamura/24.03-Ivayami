#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Ivayami.Save
{
    internal static class SaveEraseUtilityWindow
    {
        [MenuItem("Ivayami/Save/Erase Progress", false, 0)]
        public static void EraseProgressSave()
        {
            if (EditorUtility.DisplayDialog("Erase Save Progress Files", "Erase ALL Save Progress Files", "OK", "Cancel"))
            {
                string[] filesCache = Directory.GetFiles(SaveSystem.ProgressSavePath,
                        $"{SaveSystem.SaveProgressFileName}*", SearchOption.TopDirectoryOnly);
                for(int i = 0; i < filesCache.Length; i++)
                {
                    if (File.Exists(filesCache[i]))
                    {
                        File.Delete(filesCache[i]);
                        Debug.Log($"Save Progress {filesCache[i]} deleted");
                    }
                }
                if (filesCache.Length == 0) Debug.Log("No Save Progress to delete");
            }
        }

        [MenuItem("Ivayami/Save/Erase Options Preferences", false, 0)]
        public static void EraseOptionsSave()
        {
            if (EditorUtility.DisplayDialog("Erase Save Options File", "Erase Save Options", "OK", "Cancel"))
            {
                string[] filesCache = Directory.GetFiles(Application.persistentDataPath, $"{SaveSystem.SaveOptionsFileName}*");
                for (int i = 0; i < filesCache.Length; i++)
                {
                    if (File.Exists(filesCache[i]))
                    {
                        File.Delete(filesCache[i]);
                        Debug.Log($"Save Options {filesCache[i]} deleted");
                    }
                }
                if(filesCache.Length == 0) Debug.Log("No Save Options to delete");
            }
        }
    }
}
#endif