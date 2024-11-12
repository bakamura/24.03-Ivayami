using System.Linq;
using UnityEngine;

namespace Ivayami.Player {
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject {
        
        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public ItemType Type { get; protected set; }
        [field : SerializeField] public ItemUsageAction UsageAction { get; protected set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public GameObject Model { get; private set; }

        public InventoryItem GetTranslation(LanguageTypes language) {
            if (language == LanguageTypes.ENUS) return this;
            InventoryItem inventoryItem = Resources.LoadAll<InventoryItem>($"Items/{language}").First(item => item.name == name);
            Resources.UnloadUnusedAssets();
            if (inventoryItem != null) return inventoryItem;
            else {
                Debug.LogError($"No translation {language} found of '{name}' (InventoryItem)");
                return this;
            }
        }
    }
}
