using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System;

namespace Ivayami.Player {
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItem : ScriptableObject {

#if UNITY_EDITOR
        public TextContent[] DisplayTexts;
#endif
        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public ItemType Type { get; protected set; }
        [field : SerializeField, Tooltip("Only supported when the item type is Consumable")] public ItemUsageAction UsageAction { get; protected set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public GameObject Model { get; private set; }

        [Serializable]
        public struct TextContent
        {
            [ReadOnly] public string Language;
            public string Name;
            public string Description;
        }

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (DisplayTexts == null || DisplayTexts.Length == 0) return;
            int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;
            if (languagesCount > 0 && DisplayTexts.Length != languagesCount)
            {
                Array.Resize(ref DisplayTexts, languagesCount);
                for(int i = 0; i < DisplayTexts.Length; i++)
                {
                    DisplayTexts[i].Language = LocalizationSettings.AvailableLocales.Locales[i].LocaleName;
                }
            }
        }

        //[ContextMenu("Fix")]
        //private void FixTest()
        //{
        //    int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;
        //    InventoryItem[] assetsEN = Resources.LoadAll<InventoryItem>("Items/ENUS");
        //    InventoryItem[] assetsPT = Resources.LoadAll<InventoryItem>("Items/PTBR");
        //    for (int i = 0; i < assetsEN.Length; i++)
        //    {
        //        assetsEN[i].DisplayTexts = new TextContent[languagesCount];
        //        assetsEN[i].DisplayTexts[0].Name = assetsEN[i].DisplayName;
        //        assetsEN[i].DisplayTexts[0].Description = assetsEN[i].Description;
        //        for(int a = 0; a < assetsEN[i].DisplayTexts.Length; a++)
        //        {
        //            assetsEN[i].DisplayTexts[a].Language = LocalizationSettings.AvailableLocales.Locales[a].LocaleName;
        //        }
        //        UnityEditor.EditorUtility.SetDirty(assetsEN[i]);
        //    }
        //    for (int i = 0; i < assetsEN.Length; i++)
        //    {
        //        assetsEN[i].DisplayTexts[1].Name = assetsPT[i].DisplayName;
        //        assetsEN[i].DisplayTexts[1].Description = assetsPT[i].Description;
        //        UnityEditor.EditorUtility.SetDirty(assetsEN[i]);
        //    }
        //}
#endif
    }
}
