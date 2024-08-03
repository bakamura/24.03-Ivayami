using Ivayami.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ivayami.UI
{
    [RequireComponent(typeof(Fade))]
    public class KeyTutorial : MonoBehaviour
    {
        [SerializeField] private InputActionReference _actionIndicator;
        [SerializeField] private Sprite _indicatorKeyboard;
        [SerializeField] private Sprite _indicatorGamepad;

        private Image _icon
        {
            get
            {
                if (!m_icon) m_icon = GetComponentInChildren<Image>();
                return m_icon;
            }
        }
        private Image m_icon;
        private Fade _fadeUI
        {
            get
            {
                if (!m_fadeUI) m_fadeUI = GetComponentInChildren<Fade>();
                return m_fadeUI;
            }
        }
        private Fade m_fadeUI;
        public void StartTutorial()
        {
            //GameObject instance = Instantiate(_uiPrefab, FindObjectOfType<InfoUpdateIndicator>().GetComponentInChildren<Fade>().transform);
            //_icon = instance.GetComponentInChildren<Image>();
            _fadeUI.Open();
            UpdateVisuals(InputCallbacks.Instance.CurrentControlScheme.Equals("Gamepad"));
            InputCallbacks.Instance.AddEventToOnChangeControls(HandleChangeControls);
            _actionIndicator.action.performed += KeyPressed;
        }

        private void HandleChangeControls(PlayerInput script)
        {
            UpdateVisuals(script.currentControlScheme.Equals("Gamepad"));
        }

        private void UpdateVisuals(bool isGamepad)
        {
            _icon.sprite = isGamepad ? _indicatorGamepad : _indicatorKeyboard;
        }

        private void KeyPressed(InputAction.CallbackContext obj)
        {
            _actionIndicator.action.performed -= KeyPressed;
            _fadeUI.Close();
        }
        //private void KeyPressed()
        //{
        //    _actionIndicator.action.performed -= (callbackContext) => KeyPressed(instance);
        //    InputCallbacks.Instance.RemoveEventToOnChangeControls((callbackContext) => KeyPressed(instance));
        //    Destroy(instance);
        //}

    }
}