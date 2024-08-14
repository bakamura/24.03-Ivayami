using UnityEngine;
using System;
using UnityEngine.UI;

namespace Ivayami.Puzzle
{
    //[RequireComponent(typeof(UiLanguage))]
    public class PuzzleInputDisplayUI : MonoBehaviour
    {
        [SerializeField] private DisplayInfo[] _displays;
        [Serializable]
        private struct DisplayInfo
        {
            public Image Icon;
            public Sprite KeyboardIcon;
            public Sprite GamepadIcon;
        }

        //private UiLanguage _uiLanguage
        //{
        //    get
        //    {
        //        if (!m_uiLanguage) m_uiLanguage = GetComponent<UiLanguage>();
        //        return m_uiLanguage;
        //    }
        //}
        //private UiLanguage m_uiLanguage;

        private void UpdateVisuals(bool isGamepad)
        {
            for (int i = 0; i < _displays.Length; i++)
            {
                _displays[i].Icon.sprite = isGamepad ? _displays[i].GamepadIcon : _displays[i].KeyboardIcon;
            }
        }

        private void OnEnable()
        {
            if (InputCallbacks.Instance) InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateVisuals);
            //if (SaveSystem.Instance && SaveSystem.Instance.Options != null) _uiLanguage.UpdateLanguage((LanguageTypes)SaveSystem.Instance.Options.language);
        }

        private void OnDisable()
        {
            if (InputCallbacks.Instance) InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisuals);
        }
    }
}