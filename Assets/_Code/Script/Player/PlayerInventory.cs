using Ivayami.UI;
using System.Collections.Generic;

namespace Ivayami.Player {
    public class PlayerInventory : MonoSingleton<PlayerInventory> {

        private List<InventoryItem> _itemList = new List<InventoryItem>();

        private int _checkInventoryIndexCache;

        public InventoryItem[] CheckInventory() {
            return _itemList.ToArray();
        }

        public InventoryItem CheckInventoryFor(string itemId) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.name == itemId);
            return _checkInventoryIndexCache == -1 ? null : _itemList[_checkInventoryIndexCache];
        }

        public void AddToInventory(InventoryItem item, bool shouldEmphasize = false) {
            _itemList.Add(item);
            if (shouldEmphasize) ;
            else InfoUpdateIndicator.Instance.DisplayUpdate(item.Sprite);

            Logger.Log(LogType.Player, $"Inventory Add: {item.DisplayName} ({item.name}) / {item.Type}");
        }

        public void RemoveFromInventory(InventoryItem item) {
            _itemList.Remove(item);

            Logger.Log(LogType.Player, $"Inventory Remove: {item.DisplayName} ({item.name}) / {item.Type}");
        }

    }
}