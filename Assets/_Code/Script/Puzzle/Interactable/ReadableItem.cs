using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.UI;
using Ivayami.Save;

namespace Ivayami.Player {
    public class ReadableItem : InventoryItem {
 
        public JournalEntry JournalEntry { get; private set; }

        public ReadableItem(string readableName) {
            name = readableName;
            Readable readable = Resources.Load<Readable>($"Readable/ENUS/{name}").GetTranslation(SaveSystem.Instance.Options.Language);
            JournalEntry = new JournalEntry(readable.Title, readable.Content);
        }

    }
}