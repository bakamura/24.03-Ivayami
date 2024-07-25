
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
            if(progress.ContainsKey(type)) progress[type] = amount;
            else progress.Add(type, amount);
        }

        public int GetProgressOfType(string type) {
            if (progress.ContainsKey(type)) return progress[type];
            else return 0;
        }

    }
}