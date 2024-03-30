using UnityEngine;

namespace Paranapiacaba.Player {
    public class PlayerInventory : MonoSingleton<PlayerInventory> {

        public InventoryItem[] CheckInventory() {
            Debug.LogWarning("Method Not Implemented Yet");
            return null;
        }

        public InventoryItem CheckInventoryFor(string itemId) {
            Debug.LogWarning("Method Not Implemented Yet");
            return null;
        }

        public void AddToInventory(InventoryItem item) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

        public void RemoveFromInventory(InventoryItem item) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}