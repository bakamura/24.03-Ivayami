using UnityEngine;
using Ivayami.Player;
using Ivayami.Save;
using System.Linq;

namespace Ivayami.UI
{
    [CreateAssetMenu(menuName = "PlayProfile", fileName = "NewPlayProfile")]
    public class PlayProfile : ScriptableObject
    {
        //[Tooltip("The index of this profile in the progression of the game, higher values indicate further progression in game")]public uint ProfileOrderInProgression;
        //[Tooltip("If true will apply all previous profiles Items, AreaProgress and EntryProgress")] public bool UseLowerProgressionProfilesToUpdateProgression;
        public Vector3 PlayerStartPosition;
        [Range(-180f, 180f)] public float PlayerStartRotation;
        [Min(-1), Tooltip("-1 equals no save point")] public int InitialSavePoint = -1;
        [Range(0f, 99f)] public float InitialStress;
        //public bool OnlySaveSpawnPosition;
        public PlayerInventory.InventoryItemStack[] Items;
        public AreaProgressData[] AreaProgress;
        public AreaProgressData[] EntryProgress;
        private AreaProgress[] _entryProgressData;
        //private PlayProfile[] _playProfiles;

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

            //if (_playProfiles == null || _playProfiles.Length == 0) _playProfiles = Resources.LoadAll<PlayProfile>("PlayProfiles");
            //if(_playProfiles.FirstOrDefault(x => x.ProfileOrderInProgression == ProfileOrderInProgression && x != this) != null)
            //{
            //    for(int i = 0; i < )
            //}
        }
    }
}