using Ivayami.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Player {
    public class PlayerInventory : MonoSingleton<PlayerInventory> {

        public UnityEvent<InventoryItem[]> onInventoryUpdate = new UnityEvent<InventoryItem[]>();

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
            onInventoryUpdate.Invoke(CheckInventory());
            if (shouldEmphasize) ;
            else InfoUpdateIndicator.Instance.DisplayUpdate(item.Sprite);

            Logger.Log(LogType.Player, $"Inventory Add: {item.DisplayName} ({item.name}) / {item.Type}");
        }

        public void RemoveFromInventory(InventoryItem item) {
            _itemList.Remove(item);
            onInventoryUpdate.Invoke(CheckInventory());

            Logger.Log(LogType.Player, $"Inventory Remove: {item.DisplayName} ({item.name}) / {item.Type}");
        }

        public void LoadInventory(string[] itemNames) {
            _itemList.Clear();
            if (itemNames?.Length > 0) {
                InventoryItem[] itemAssets = Resources.LoadAll<InventoryItem>($"Items/ENUS");
                foreach (string itemName in itemNames) _itemList.Add(itemAssets.First(asset => asset.name == itemName));
            }
        }

    }
}