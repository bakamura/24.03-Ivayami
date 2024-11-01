#if UNITY_EDITOR
using System;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.UI;
using Ivayami.Save;

namespace Ivayami.Player {
    public class PlayerInventory : MonoSingleton<PlayerInventory> {

        public UnityEvent<InventoryItem[]> onInventoryUpdate = new UnityEvent<InventoryItem[]>();

        private List<InventoryItem> _itemList = new List<InventoryItem>();
        [SerializeField] private Sprite[] _itemTypeDefaultIcons;
        public Dictionary<ItemType, Sprite> ItemTypeDefaultIcons { get; private set; } = new Dictionary<ItemType, Sprite>();

        private int _checkInventoryIndexCache;

        protected override void Awake() {
            base.Awake();

            for (int i = 0; i < _itemTypeDefaultIcons.Length; i++) ItemTypeDefaultIcons.Add((ItemType)i, _itemTypeDefaultIcons[i]);
        }

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
            InventoryItem itemTranslation = item.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language);
            if (shouldEmphasize) ItemEmphasisDisplay.Instance.DisplayItem(item.Sprite, itemTranslation.DisplayName, itemTranslation.Description);
            else InfoUpdateIndicator.Instance.DisplayUpdate(item.Sprite, itemTranslation.DisplayName);

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
                InventoryItem iterator;
                foreach (string itemName in itemNames) {
                    iterator = itemAssets.First(asset => asset.name == itemName);
                    if (iterator != null) _itemList.Add(iterator);
                    else new ReadableItem(itemName);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_itemTypeDefaultIcons == null || _itemTypeDefaultIcons.Length != Enum.GetNames(typeof(ItemType)).Length) Array.Resize(ref _itemTypeDefaultIcons, Enum.GetNames(typeof(ItemType)).Length);
        }
#endif

    }
}