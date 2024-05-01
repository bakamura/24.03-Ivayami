
namespace Ivayami.Save {
    [System.Serializable]
    public class SaveProgress {

        public byte id;
        public byte currentChapter;
        public byte currentSubChapter;
        public string[] inventory;
        public string lastPlayedDate;

        public SaveProgress(byte id) {
            this.id = id;
        }

    }
}