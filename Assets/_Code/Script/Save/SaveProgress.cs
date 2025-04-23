using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ivayami.Save {
    [System.Serializable]
    public class SaveProgress {

        public byte id;
        public string lastPlayedDate;

        public string[] inventory;
        public string[] abilities = new string[0];
        public int pointId;
        public SerializableDictionary<string, int> gameProgress = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, int> entryProgress = new SerializableDictionary<string, int>();
        public int[] roadBlockersState = new int[5];
        public SerializableDictionary<string, string> saveObjects = new SerializableDictionary<string, string>();

        [Serializable]
        public class ItemData {
            public string ID;
            public int Amount;

            public ItemData(string id, int amount) {
                ID = id;
                Amount = amount;
            }

            public ItemData(Player.PlayerInventory.InventoryItemStack itemStack) {
                ID = itemStack.Item.name;
                Amount = itemStack.Amount;
            }
        }

        public SaveProgress(byte id) {
            this.id = id;
        }

        public void SaveProgressOfType(string type, int amount) {
            if (gameProgress.ContainsKey(type)) {
                if (gameProgress[type] < amount) gameProgress[type] = amount;
                else Debug.LogWarning("The value is smaller then the current progress step");
            }
            else gameProgress.Add(type, amount);
        }

        public int GetProgressOfType(string type) {
            if (gameProgress.ContainsKey(type)) return gameProgress[type];
            else return 0;
        }

        public void SaveEntryProgressOfType(string type, int amount) {
            if (entryProgress.ContainsKey(type)) {
                if (entryProgress[type] < amount) entryProgress[type] = amount;
                else Debug.LogWarning("The value is smaller then the current progress step");
            }
            else entryProgress.Add(type, amount);
        }

        public int GetEntryProgressOfType(string type) {
            if (entryProgress.ContainsKey(type)) return entryProgress[type];
            else return 0;
        }

        public void AddAbility(string abilityName) {
            if (string.IsNullOrEmpty(abilityName)) {
                Debug.LogWarning("Trying to add an empty or null ability!");
                return;
            }
            if (abilities.Contains(abilityName)) {
                Debug.LogWarning($"Trying to add Ability '{abilityName}' to SaveProgress.abilities, but already contains it!");
                return;
            }
            Array.Resize(ref abilities, abilities.Length + 1);
            abilities[abilities.Length - 1] = abilityName;
        }

        public void RemoveAbility(string abilityName) {
            if (string.IsNullOrEmpty(abilityName)) {
                Debug.LogWarning("Trying to remove an empty or null ability!");
                return;
            }
            if (abilities.Length == 0) {
                Debug.LogWarning($"Trying to remove Ability '{abilityName}' from SaveProgress.abilities, but the array is empty!");
                return;
            }
            List<string> list = abilities.ToList();
            if (!list.Remove(abilityName)) {
                Debug.LogWarning($"Trying to remove Ability '{abilityName}' from SaveProgress.abilities, but doesn't contain it!");
                return;
            }
            abilities = list.ToArray();
        }

        public void SaveRoadBlockerState(int blockerId, RoadBlocker.State state) {
            if (roadBlockersState.Length <= blockerId) Array.Resize(ref roadBlockersState, blockerId);
            roadBlockersState[blockerId] = (int)state;
        }

        public RoadBlocker.State GetRoadBlockerState(int blockerId) {
            if (roadBlockersState.Length <= blockerId) Array.Resize(ref roadBlockersState, blockerId + 1);
            return (RoadBlocker.State)roadBlockersState[blockerId];
        }

        public void RecordSaveObject(string id, object data) {
            if (saveObjects.ContainsKey(id))
                saveObjects[id] = JsonUtility.ToJson(data);
            else saveObjects.Add(id, JsonUtility.ToJson(data));
        }

        public bool GetSaveObjectOfType<T>(string id, out T data) {
            if (saveObjects.ContainsKey(id)) {
                data = JsonUtility.FromJson<T>(saveObjects[id]);
                return true;
            }
            //return JsonUtility.FromJson<T>(saveObjects[id]);
            else {
                Logger.Log(LogType.Save, $"The object {id} has not been saved yet");
                data = default(T);
                return false;
                //data = null;
                //return data;
            }
        }

        public ItemData[] GetItemsData() {
            if (inventory == null) return null;
            ItemData[] items = new ItemData[inventory.Length];
            for (int i = 0; i < items.Length; i++) {
                items[i] = JsonUtility.FromJson<ItemData>(inventory[i]);
            }
            return items;
        }
    }
}