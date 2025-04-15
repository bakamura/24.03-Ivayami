using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeResolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    private int _currentIndex;

    private void Awake()
    {
        List<string> opt = new List<string>();
        for(int i = 0; i < Screen.resolutions.Length; i++)
        {
            opt.Add($"{Screen.resolutions[i].width}x{Screen.resolutions[i].height}");
            if (Screen.resolutions[i].width == Screen.currentResolution.width && Screen.resolutions[i].height == Screen.currentResolution.height)
                _currentIndex = i;
        }
        _dropdown.AddOptions(opt);
        _dropdown.SetValueWithoutNotify(_currentIndex);
    }

    public void OnValueChange(int index)
    {
        _currentIndex = index;
        Screen.SetResolution(Screen.resolutions[_currentIndex].width, Screen.resolutions[_currentIndex].height, FullScreenMode.FullScreenWindow);
    }
}
