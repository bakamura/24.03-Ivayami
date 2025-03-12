using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Player {
    public class ReadableItem : InventoryItem {
 
        //public JournalEntry JournalEntry { get; private set; }
        public Readable Entry { get; private set; }

        public ReadableItem(string readableName) {
            name = readableName;
            Entry = Resources.Load<Readable>($"Readable/{readableName}");
            //DisplayTexts = readable.DisplayTexts;
            //JournalEntry = new JournalEntry(readable);
            Type = ItemType.Document;
            Resources.UnloadUnusedAssets();
        }

    }
}