using UnityEngine;

namespace Ivayami.Player {
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject {

        public string displayName;
        public ItemType type;
        public Sprite sprite;

    }
}
