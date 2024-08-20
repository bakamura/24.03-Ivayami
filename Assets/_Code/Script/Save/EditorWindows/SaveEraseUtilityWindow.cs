#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Ivayami.Save
{
    internal static class SaveEraseUtilityWindow
    {
        [MenuItem("Ivayami/Save/Erase Progress Files", false, 0)]
        public static void EraseProgressSave()
        {
            if (EditorUtility.DisplayDialog("Erase All Progress Files", "This will erase all progress in all slots, continue?", "Continue", "Cancel"))
            {
                string[] filesCache = Directory.GetFiles($"{Application.persistentDataPath}/{SaveSystem.ProgressFolderName}", $"{SaveSystem.ProgressFolderName}*", SearchOption.TopDirectoryOnly);
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

        [MenuItem("Ivayami/Save/Erase Options File", false, 0)]
        public static void EraseOptionsSave()
        {
            if (EditorUtility.DisplayDialog("Erase Save Options", "This will erase all options saved, continue?", "Continue", "Cancel"))
            {
                string[] filesCache = Directory.GetFiles(Application.persistentDataPath, $"{SaveSystem.OptionsFileName}*");
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
