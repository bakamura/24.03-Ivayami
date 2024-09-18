using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ivayami.Dialogue;
using System.IO;
using UnityEditor;

public class ChangeSpeeches : MonoBehaviour
{
    [SerializeField] private Dialogue _toUpdate;
    [SerializeField] private string _filePath;
    [SerializeField] private LanguageTypes _currentLanguage;

    [ContextMenu("UpdateFile")]
    private void ChangeFile()
    {
        int currentIndex = -1;
        string currentFile = null;
        string[] files = Directory.GetFiles(_filePath, "*.asset", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            foreach (var line in File.ReadAllLines(files[i]))
            {
                if (line.Contains("m_Name:") && line.Split(':')[1].Trim() == _toUpdate.name)
                {
                    currentFile = files[i];
                    break;
                }
            }
            if (currentFile != null) break;
        }
        if (currentFile == null)
        {
            Debug.Log("Cant find file");
            return;
        }
        bool changed;
        foreach (var line in File.ReadAllLines(currentFile))
        {
            changed = false;
            if (line.Contains("- announcerName"))
            {
                changed = true;
                currentIndex++;
            }
            if(changed) Undo.RecordObject(_toUpdate, "Return");
            if (line.Contains("announcerName"))
            {
                _toUpdate.dialogue[currentIndex].Speeches[(int)_currentLanguage].announcerName = line.Split(':')[1].Trim(' ');                
            }
            if (line.Contains("eventId"))
            {
                _toUpdate.dialogue[currentIndex].EventId = line.Split(':')[1].Trim(' ');
            }
            if (line.Contains("content"))
            {
                _toUpdate.dialogue[currentIndex].Speeches[(int)_currentLanguage].content = line.Split(':')[1].Trim(' ');
            }
        }

    }
}
