using System;
using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;
using UnityEngine.Serialization;

namespace Ivayami.Puzzle
{
    public class PuzzleInputDisplayUI : MonoBehaviour
    {
        [SerializeField] private DisplayInfo[] _displays;
        [Serializable]
        private struct DisplayInfo
        {
            [FormerlySerializedAs("Icon")] public Image Image;
            public Sprite[] Icons;
        }

        private void UpdateVisuals(InputCallbacks.ControlType controlType)
        {
            for (int i = 0; i < _displays.Length; i++) _displays[i].Image.sprite = _displays[i].Icons[(int)controlType];
        }

        private void OnEnable()
        {
            if (InputCallbacks.Instance) InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateVisuals);
        }

        private void OnDisable()
        {
            if (InputCallbacks.Instance) InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisuals);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            int enumSize = Enum.GetNames(typeof(InputCallbacks.ControlType)).Length;
            for (int i = 0; i < _displays.Length; i++) if (_displays[i].Icons.Length != enumSize) Array.Resize(ref _displays[i].Icons, enumSize);
        }
#endif

    }
}