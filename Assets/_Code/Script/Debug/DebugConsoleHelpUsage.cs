using UnityEngine;
using IngameDebugConsole;
using UnityEngine.InputSystem;
using Ivayami.Player;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Ivayami.debug
{
    public sealed class DebugConsoleHelpUsage : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private InputActionMap _previousMap;
        private InputActionMap _menuMap;
        private CursorLockMode _previousMode;
        private Selectable _previousSelectable;

        void Start()
        {
            if (!PlayerActions.Instance) return;
            _playerInput = PlayerActions.Instance.GetComponent<PlayerInput>();
            _menuMap = _playerInput.actions.actionMaps.FirstOrDefault(x => x.name == "Menu");
            DebugLogManager.Instance.OnLogWindowShown += HandleWindowShow;
            DebugLogManager.Instance.OnLogWindowHidden += HandleWindowClose;
        }

        private void HandleWindowClose()
        {
            for (int i = 1; i <= 5; i++)
            {
                _menuMap.actions[i].Disable();
            }
            //foreach (InputActionMap map in _playerInput.actions.actionMaps)
            //{
            //    map.Disable();
            //}
            _previousMap.Enable();
            Cursor.lockState = _previousMode;
            if (_previousSelectable) _previousSelectable.Select();
        }

        private void HandleWindowShow()
        {
            _previousMap = PlayerActions.Instance.CurrentActionMap;
            _previousMode = Cursor.lockState;
            _previousSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            foreach (InputActionMap map in _playerInput.actions.actionMaps)
            {
                map.Disable();
            }
            for (int i = 1; i <= 5; i++)
            {
                _menuMap.actions[i].Enable();
            }
            Cursor.lockState = CursorLockMode.None;
        }
    }
}