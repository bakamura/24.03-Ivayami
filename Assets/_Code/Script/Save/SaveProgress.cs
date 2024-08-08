
using UnityEngine;

namespace Ivayami.Save {
    [System.Serializable]
    public class SaveProgress {

        public byte id;
        public string lastPlayedDate;
        
        public string[] inventory;
        public int pointId;
        public string lastProgressType;
        public SerializableDictionary<string, int> progress = new SerializableDictionary<string, int>();

        public SaveProgress(byte id) {
            this.id = id;
        }

        public void SaveProgressOfType(string type, int amount) {
            if (progress.ContainsKey(type)) {
                if (progress[type] < amount) progress[type] = amount;
                else Debug.LogWarning("The value is smaller then the current progress step");
            }
            else progress.Add(type, amount);
        }

        public int GetProgressOfType(string type) {
            if (progress.ContainsKey(type)) return progress[type];
            else return 0;
        }

    }
}