using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Ivayami.Player;
using Ivayami.UI;

namespace Ivayami.Puzzle
{
    public class PuzzleInputDisplayUI : MonoBehaviour
    {
        [SerializeField] private DisplayInfo[] _displays;
        [Serializable]
        private struct DisplayInfo
        {
            [FormerlySerializedAs("Icon")] public Image Image;
            public InputIcons InputIcons;
        }

        private void UpdateVisuals(InputCallbacks.ControlType controlType)
        {
            for (int i = 0; i < _displays.Length; i++) _displays[i].Image.sprite = _displays[i].InputIcons.Icons[(int)controlType];
        }

        private void OnEnable()
        {
            if (InputCallbacks.Instance) InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateVisuals);
        }

        private void OnDisable()
        {
            if (InputCallbacks.Instance) InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisuals);
        }

    }
}