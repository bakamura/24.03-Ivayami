using System.Collections.Generic;

namespace Paranapiacaba.Player {
    public class PlayerInventory : MonoSingleton<PlayerInventory> {

        private List<InventoryItem> _itemList = new List<InventoryItem>();

        private int _checkInventoryIndexCache;

        public InventoryItem[] CheckInventory() {
            return _itemList.ToArray();
        }

        public InventoryItem CheckInventoryFor(string itemId) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.id == itemId);
            return _checkInventoryIndexCache == -1 ? null : _itemList[_checkInventoryIndexCache];
        }

        public void AddToInventory(InventoryItem item) {
            _itemList.Add(item);

            Logger.Log(LogType.Player, $"Inventory Add: {item.name} ({item.id}) / {item.type}");
        }

        public void RemoveFromInventory(InventoryItem item) {
            _itemList.Remove(item);

            Logger.Log(LogType.Player, $"Remove Add: {item.name} ({item.id}) / {item.type}");
        }

    }
}