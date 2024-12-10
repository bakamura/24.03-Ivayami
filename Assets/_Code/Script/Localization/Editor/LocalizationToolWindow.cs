using UnityEngine;
using UnityEditor;
using UnityEditor.Localization;
using Ivayami.Player;
using UnityEngine.Localization.Tables;
using System.Linq;
using Ivayami.Puzzle;
using Ivayami.UI;

namespace Ivayami.Localization
{
    internal sealed class LocalizationToolWindow : EditorWindow
    {
        [SerializeField] private TableType _tableType;

        private const string _dialogueTableName = "Dialogues";
        private const string _itemTableName = "Items";
        //private const string _uiTableName = "UI";
        private const string _journalTableName = "Journal";
        private enum TableType
        {
            Dialogue,
            Item,
            //UI,
            Journal
        }

        [MenuItem("Ivayami/Localization/Table Update")]
        private static void ShowWindow()
        {
            var window = GetWindow<LocalizationToolWindow>();
            window.titleContent = new GUIContent("Tables Update");
            window.Show();
        }

        private void OnGUI()
        {
            _tableType = (TableType)EditorGUILayout.EnumPopup("Table Type", _tableType);

            if (GUILayout.Button("Update Table"))
            {
                UpdateLocalizationTable();
            }
        }

        private void UpdateLocalizationTable()
        {
            byte index = 0;
            switch (_tableType)
            {
                case TableType.Dialogue:
                    Dialogue.Dialogue[] dialogues = Resources.LoadAll<Dialogue.Dialogue>("Dialogues");
                    //places in alphabetic order
                    dialogues = dialogues.OrderBy(x => x.name).ToArray();
                    LocalizationEditorSettings.GetStringTableCollection(_dialogueTableName).ClearAllEntries();
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_dialogueTableName).StringTables)
                    {
                        for(int i = 0; i < dialogues.Length; i++)
                        {
                            for(int a = 0; a < dialogues[i].dialogue.Length; a++)
                            {
                                table.AddEntry($"{dialogues[i].name}/Speech_{a}", dialogues[i].dialogue[a].Speeches[index].content);
                            }
                        }
                        index++;
                    }
                    EditorUtility.SetDirty(LocalizationEditorSettings.GetStringTableCollection(_dialogueTableName));
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_dialogueTableName).StringTables)
                    {
                        EditorUtility.SetDirty(table);
                    }
                    Debug.Log("Dialogue Table Updated");
                    break;
                case TableType.Item:
                    InventoryItem[] items = Resources.LoadAll<InventoryItem>("Items");
                    Readable[] readables = Resources.LoadAll<Readable>("Readable");
                    //places in alphabetic order
                    items = items.OrderBy(x => x.name).ToArray();
                    readables = readables.OrderBy(x => x.name).ToArray();
                    LocalizationEditorSettings.GetStringTableCollection(_itemTableName).ClearAllEntries();
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_itemTableName).StringTables)
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            table.AddEntry($"{items[i].name}/Name", items[i].DisplayTexts[index].Name);
                            table.AddEntry($"{items[i].name}/Description", items[i].DisplayTexts[index].Description);
                        }
                        for(int i = 0; i < readables.Length; i++)
                        {
                            table.AddEntry($"{readables[i].name}/Name", readables[i].DisplayTexts[index].Name);
                            table.AddEntry($"{readables[i].name}/Description", readables[i].DisplayTexts[index].Description);
                        }
                        index++;
                    }
                    EditorUtility.SetDirty(LocalizationEditorSettings.GetStringTableCollection(_itemTableName));
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_itemTableName).StringTables)
                    {
                        EditorUtility.SetDirty(table);
                    }
                    Debug.Log("Item Table Updated");
                    break;
                case TableType.Journal:
                    JournalEntry[] entries = Resources.LoadAll<JournalEntry>("Journal");
                    //places in alphabetic order
                    entries = entries.OrderBy(x => x.name).ToArray();
                    LocalizationEditorSettings.GetStringTableCollection(_journalTableName).ClearAllEntries();
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_journalTableName).StringTables)
                    {
                        for (int i = 0; i < entries.Length; i++)
                        {
                            table.AddEntry($"{entries[i].name}/Name", entries[i].DisplayTexts[index].Name);
                            for (int a = 0; a < entries[i].DisplayTexts[index].Descriptions.Length; a++)
                            {
                                table.AddEntry($"{entries[i].name}/Description_{a}", entries[i].DisplayTexts[index].Descriptions[a]);
                            }
                        }
                        index++;
                    }
                    EditorUtility.SetDirty(LocalizationEditorSettings.GetStringTableCollection(_journalTableName));
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_journalTableName).StringTables)
                    {
                        EditorUtility.SetDirty(table);
                    }
                    Debug.Log("Journal Table Updated");
                    break;
                //case TableType.UI:
                //    break;
            }
        }
    }
}