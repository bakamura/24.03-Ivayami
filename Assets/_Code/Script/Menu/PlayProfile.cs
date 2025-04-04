using UnityEngine;
using Ivayami.Player;
using Ivayami.Save;
using System.Linq;

namespace Ivayami.UI
{
    [CreateAssetMenu(menuName = "PlayProfile", fileName = "NewPlayProfile")]
    public class PlayProfile : ScriptableObject
    {
        public Vector3 PlayerStartPosition;
        [Range(-180f, 180f)] public float PlayerStartRotation;
        [Range(0f, 99f)] public float InitialStress;
        public bool SaveIsActive;
        public PlayerInventory.InventoryItemStack[] Items;
        public AreaProgressData[] AreaProgress;
        public AreaProgressData[] EntryProgress;
        private AreaProgress[] _entryProgressData;

        [System.Serializable]
        public struct AreaProgressData
        {
            public AreaProgress AreaProgress;
            public int Step;
        }

        private void OnValidate()
        {
            if (_entryProgressData == null || _entryProgressData.Length == 0) _entryProgressData = Resources.LoadAll<AreaProgress>("AreaProgress/JournalEntry");
            if (AreaProgress != null)
            {
                for (int i = 0; i < AreaProgress.Length; i++)
                {
                    if (AreaProgress[i].AreaProgress && _entryProgressData.Contains(AreaProgress[i].AreaProgress))
                    {
                        Debug.LogWarning($"The file {AreaProgress[i].AreaProgress.Id} is an EntryProgress file, please put it in the EntryProgress List");
                        AreaProgress[i].AreaProgress = null;
                    }
                }
            }
            if (EntryProgress != null)
            {
                for (int i = 0; i < EntryProgress.Length; i++)
                {
                    if (EntryProgress[i].AreaProgress && !_entryProgressData.Contains(EntryProgress[i].AreaProgress))
                    {
                        Debug.LogWarning($"The file {EntryProgress[i].AreaProgress.Id} is an AreaProgress file, please put it in the AreaProgress List");
                        EntryProgress[i].AreaProgress = null;
                    }
                }
            }
        }
    }
}