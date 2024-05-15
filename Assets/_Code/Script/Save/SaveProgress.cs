
namespace Ivayami.Save {
    [System.Serializable]
    public class SaveProgress {

        public byte id;
        public string lastPlayedDate;
        
        public string[] inventory;
        public int pointId; 
        public SerializableDictionary<string, int> progress;

        public SaveProgress(byte id) {
            this.id = id;
        }

    }
}