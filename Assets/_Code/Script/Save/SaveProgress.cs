
namespace Paranapiacaba.Save {
    [System.Serializable]
    public class SaveProgress {

        public byte id;
        public byte currentChapter;
        public byte currentSubChapter;
        public string[] inventory;

        public SaveProgress(byte id) {
            this.id = id;
        }

    }
}