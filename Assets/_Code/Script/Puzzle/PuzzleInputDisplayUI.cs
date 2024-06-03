using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
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

        private void HandleDeviceUpdate(InputDevice device, InputDeviceChange change)
        {
            if (device.GetType() == typeof(Gamepad) && (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected))
            {
                UpdateVisuals(true);
            }
            else if (device.GetType() == typeof(Keyboard) && (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected))
            {
                UpdateVisuals(false);
            }
        }

        private void UpdateVisuals(bool isGamepad)
        {
            for (int i = 0; i < _displays.Length; i++)
            {
                _displays[i].Icon.sprite = isGamepad ? _displays[i].GamepadIcon : _displays[i].KeyboardIcon;
            }
        }

        private void OnEnable()
        {
            UpdateVisuals(false);
            InputSystem.onDeviceChange += HandleDeviceUpdate;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= HandleDeviceUpdate;
        }
    }
}