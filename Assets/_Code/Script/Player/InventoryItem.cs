using UnityEngine;

namespace Ivayami.Player {
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject {

        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }

    }
}
