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
        public Action<InventoryItemStack> onItemStackUpdate;

        private List<InventoryItemStack> _itemList = new List<InventoryItemStack>();
        [SerializeField] private Sprite[] _itemTypeDefaultIcons;
        public Dictionary<ItemType, Sprite> ItemTypeDefaultIcons { get; private set; } = new Dictionary<ItemType, Sprite>();

        private int _checkInventoryIndexCache;

        [Serializable]
        public struct InventoryItemStack
        {
            public InventoryItem Item;
            public int Amount;
            public static InventoryItemStack Empty = new InventoryItemStack();
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

        public void AddToInventory(InventoryItem item, bool shouldEmphasize = false, bool shouldPlayFeedbacks = true) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.Item.name == item.name);
            InventoryItemStack itemStack;
            if (_checkInventoryIndexCache == -1)
            {
                itemStack = new InventoryItemStack(item, 1);
                _itemList.Add(itemStack);
            }
            else
            {
                itemStack = new InventoryItemStack(_itemList[_checkInventoryIndexCache].Item, _itemList[_checkInventoryIndexCache].Amount + 1);
                _itemList[_checkInventoryIndexCache] = itemStack;
            }
            onInventoryUpdate.Invoke(CheckInventory());
            onItemStackUpdate?.Invoke(itemStack);
            if (shouldPlayFeedbacks)
            {
                if (shouldEmphasize) ItemEmphasisDisplay.Instance.DisplayItem(item.Sprite,
                    item.GetDisplayName(),
                    item.GetDisplayDescription());
                else InfoUpdateIndicator.Instance.DisplayUpdate(item.Sprite, item.GetDisplayName());
            }

            Logger.Log(LogType.Player, $"Inventory Add: {item.GetDisplayName()} ({item.name}) / {item.Type}. Current owned {_itemList[_checkInventoryIndexCache == -1 ? 0 : _checkInventoryIndexCache].Amount}");
        }

        public void RemoveFromInventory(InventoryItem item) {
            _checkInventoryIndexCache = _itemList.FindIndex((inventoryItem) => inventoryItem.Item.name == item.name);
            bool itemRemoved = false;
            if (_checkInventoryIndexCache == -1) return;
            else
            {
                InventoryItemStack itemStack = new InventoryItemStack(_itemList[_checkInventoryIndexCache].Item, _itemList[_checkInventoryIndexCache].Amount - 1);
                _itemList[_checkInventoryIndexCache] = itemStack;
                if (_itemList[_checkInventoryIndexCache].Amount <= 0)
                {
                    itemRemoved = true;
                    _itemList.RemoveAt(_checkInventoryIndexCache);
                }
                onItemStackUpdate?.Invoke(itemStack);
            }
            onInventoryUpdate.Invoke(CheckInventory());

            Logger.Log(LogType.Player, $"Inventory Remove: {item.GetDisplayName()} ({item.name}) / {item.Type}. Current owned {(itemRemoved ? 0 : _itemList[_checkInventoryIndexCache].Amount)}");
        }

        public void LoadInventory(SaveProgress.ItemData[] itemsData) {
            _itemList.Clear();
            if (itemsData?.Length > 0) {
                InventoryItem[] itemAssets = Resources.LoadAll<InventoryItem>($"Items");
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