using UnityEngine;
using UnityEngine.Localization.Settings;
using Ivayami.Localization;

namespace Ivayami.Player {
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject {

#if UNITY_EDITOR
        public TextContent[] DisplayTexts;
#endif
        [field: SerializeField] public ItemType Type { get; protected set; }
        [field : SerializeField, Tooltip("Only supported when the item type is Consumable")] public ItemUsageAction UsageAction { get; protected set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public GameObject Model { get; private set; }


        public string GetDisplayName()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Name");
        }

        public string GetDisplayDescription()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Items", $"{name}/Description");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (DisplayTexts == null || DisplayTexts.Length == 0) return;
            int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;
            if (languagesCount > 0 && DisplayTexts.Length != languagesCount)
            {
                System.Array.Resize(ref DisplayTexts, languagesCount);
                for(int i = 0; i < DisplayTexts.Length; i++)
                {
                    DisplayTexts[i].Language = LocalizationSettings.AvailableLocales.Locales[i].LocaleName;
                }
            }
        }        
#endif
    }
}
