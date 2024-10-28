using System.Linq;
using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.UI;
using Ivayami.Save;

namespace Ivayami.Player {
    public class ReadableItem : InventoryItem {
 
        public JournalEntry JournalEntry { get; private set; }

        public ReadableItem(string readableName) {
            name = readableName;
            Readable readable = Resources.LoadAll<Readable>($"Readable/ENUS").First(readable => readable.name == name).GetTranslation(SaveSystem.Instance.Options.Language);
            DisplayName = readable.Title;
            JournalEntry = new JournalEntry(readable.Title, readable.Content);
            Type = ItemType.Document;
            Resources.UnloadUnusedAssets();
        }

    }
}