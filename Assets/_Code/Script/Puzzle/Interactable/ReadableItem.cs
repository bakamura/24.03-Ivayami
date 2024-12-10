using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.UI;

namespace Ivayami.Player {
    public class ReadableItem : InventoryItem {
 
        public JournalEntry JournalEntry { get; private set; }

        public ReadableItem(string readableName) {
            name = readableName;
            Readable readable = Resources.Load<Readable>($"Readable/{readableName}");
            //DisplayTexts = readable.DisplayTexts;
            JournalEntry = new JournalEntry(readable.GetDisplayName(), readable.GetDisplayDescription());
            Type = ItemType.Document;
            Resources.UnloadUnusedAssets();
        }

    }
}