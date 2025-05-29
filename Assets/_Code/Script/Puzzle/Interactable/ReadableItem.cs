using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Player {
    public class ReadableItem : InventoryItem {
 
        public Readable Entry { get; private set; }

        public ReadableItem(string readableName) {
            name = readableName;
            Entry = Resources.Load<Readable>($"Readable/{readableName}");
            Type = ItemType.Document;
            Resources.UnloadUnusedAssets();
        }

    }
}