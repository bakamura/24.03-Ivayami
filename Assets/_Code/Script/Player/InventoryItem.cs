using UnityEngine;

namespace Paranapiacaba.Player {
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject {

        public string displayName;
        public ItemType type;
        public Sprite sprite;

    }
}
