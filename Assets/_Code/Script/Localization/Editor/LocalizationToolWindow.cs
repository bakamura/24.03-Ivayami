using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Localization;
using Ivayami.Dialogue;
using Ivayami.Player;
using UnityEngine.Localization.Tables;
using System.Linq;

namespace Ivayami.Localization
{
    internal sealed class LocalizationToolWindow : EditorWindow
    {
        [SerializeField] private TableType _tableType;

        private const string _dialogueTableName = "Dialogues";
        private const string _itemTableName = "Items";
        private const string _uiTableName = "UI";
        private enum TableType
        {
            Dialogue,
            Item,
            UI
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
            switch (_tableType)
            {
                case TableType.Dialogue:
                    Dialogue.Dialogue[] dialogues = Resources.LoadAll<Dialogue.Dialogue>("Dialogues");
                    //places in alphabetic order
                    dialogues = dialogues.OrderBy(x => x.ID).ToArray();
                    LocalizationEditorSettings.GetStringTableCollection(_dialogueTableName).ClearAllEntries();
                    byte index = 0;
                    foreach (StringTable table in LocalizationEditorSettings.GetStringTableCollection(_dialogueTableName).StringTables)
                    {
                        for(int i = 0; i < dialogues.Length; i++)
                        {
                            for(int a = 0; a < dialogues[i].dialogue.Length; a++)
                            {
                                table.AddEntry($"{dialogues[i].name}/Announcer_{a}", dialogues[i].dialogue[a].Speeches[index].announcerName);
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
                    break;
                case TableType.UI:
                    break;
            }
        }
    }
}