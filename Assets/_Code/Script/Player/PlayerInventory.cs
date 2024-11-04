using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.UI;
using Ivayami.Save;

namespace Ivayami.Player {
    public class PlayerInventory : MonoSingleton<PlayerInventory> {

        public UnityEvent<InventoryItemStack[]> onInventoryUpdate = new UnityEvent<InventoryItemStack[]>();

        private List<InventoryItemStack> _itemList = new List<InventoryItemStack>();
        [SerializeField] private Sprite[] _itemTypeDefaultIcons;
        [field: SerializeField] public string HealthItemId { get; private set; }
        public Dictionary<ItemType, Sprite> ItemTypeDefaultIcons { get; private set; } = new Dictionary<ItemType, Sprite>();

        private int _checkInventoryIndexCache;

        [Serializable]
        public struct InventoryItemStack
        {
            public InventoryItem Item;
            public int Amount;

            public InventoryItemStack(InventoryItem item, int amount)
            {
                Item = item;
                Amount = amount;
            }
        }

        protected override void Awake() {
            base.Awake();

            for (int i = 0; i < _itemTypeDefaultIcons.Length; i++) ItemTypeDefaultIcons.Add((ItemType)i, _itemTypeDefaultIcons[i]);
        }

        public InventoryItemStack[] CheckInventory() {
            return _itemList.ToArray();
        }

        public InventoryItemStack CheckInventoryFor(string itemId) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.Item.name == itemId);
            return _checkInventoryIndexCache == -1 ? new InventoryItemStack() : _itemList[_checkInventoryIndexCache];
        }

        public void AddToInventory(InventoryItem item, bool shouldEmphasize = false) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.Item.name == item.name);
            if (_checkInventoryIndexCache == -1) _itemList.Add(new InventoryItemStack(item, 1));
            else _itemList[_checkInventoryIndexCache] = new InventoryItemStack(_itemList[_checkInventoryIndexCache].Item, _itemList[_checkInventoryIndexCache].Amount + 1);
            onInventoryUpdate.Invoke(CheckInventory());
            InventoryItem itemTranslation = item.GetTranslation((LanguageTypes)SaveSystem.Instance.Options.language);
            if (shouldEmphasize) ItemEmphasisDisplay.Instance.DisplayItem(item.Sprite, itemTranslation.DisplayName, itemTranslation.Description);
            else InfoUpdateIndicator.Instance.DisplayUpdate(item.Sprite, itemTranslation.DisplayName);

            Logger.Log(LogType.Player, $"Inventory Add: {item.DisplayName} ({item.name}) / {item.Type}. Current owned {_itemList[_checkInventoryIndexCache == -1 ? 0 : _checkInventoryIndexCache].Amount}");
        }

        public void RemoveFromInventory(InventoryItem item) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.Item.name == item.name);
            bool itemRemoved = false;
            if (_checkInventoryIndexCache == -1) return;
            else
            {
                _itemList[_checkInventoryIndexCache] = new InventoryItemStack(_itemList[_checkInventoryIndexCache].Item, _itemList[_checkInventoryIndexCache].Amount - 1);
                if (_itemList[_checkInventoryIndexCache].Amount <= 0)
                {
                    itemRemoved = true;
                    _itemList.RemoveAt(_checkInventoryIndexCache);
                }
            }
            onInventoryUpdate.Invoke(CheckInventory());

            Logger.Log(LogType.Player, $"Inventory Remove: {item.DisplayName} ({item.name}) / {item.Type}. Current owned {(itemRemoved ? 0 : _itemList[_checkInventoryIndexCache].Amount)}");
        }

        public void LoadInventory(SaveProgress.ItemData[] itemsData) {
            _itemList.Clear();
            if (itemsData?.Length > 0) {
                InventoryItem[] itemAssets = Resources.LoadAll<InventoryItem>($"Items/ENUS");
                InventoryItem iterator;
                for(int i = 0; i < itemsData.Length; i++)
                {
                    iterator = itemAssets.First(asset => asset.name == itemsData[i].ID);
                    if (iterator) _itemList.Add(new InventoryItemStack(iterator, itemsData[i].Amount));
                    //else new ReadableItem(itemsData[i].ID);
                }
                Resources.UnloadUnusedAssets();
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_itemTypeDefaultIcons == null || _itemTypeDefaultIcons.Length != Enum.GetNames(typeof(ItemType)).Length) Array.Resize(ref _itemTypeDefaultIcons, Enum.GetNames(typeof(ItemType)).Length);
        }
#endif

    }
}