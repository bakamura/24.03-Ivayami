using Ivayami.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ivayami.UI {
    [RequireComponent(typeof(Fade))]
    public class KeyTutorial : MonoBehaviour {

        [SerializeField] private InputActionReference _actionIndicator;
        [SerializeField] private Sprite _indicatorKeyboard;
        [SerializeField] private Sprite _indicatorGamepad;

        private Image m_icon;
        private Image _icon {
            get {
                if (!m_icon) m_icon = GetComponentInChildren<Image>();
                return m_icon;
            }
        }
        private Fade m_fadeUI;
        private Fade _fadeUI {
            get {
                if (!m_fadeUI) m_fadeUI = GetComponentInChildren<Fade>();
                return m_fadeUI;
            }
        }

        public void StartTutorial() {
            //GameObject instance = Instantiate(_uiPrefab, FindObjectOfType<InfoUpdateIndicator>().GetComponentInChildren<Fade>().transform);
            //_icon = instance.GetComponentInChildren<Image>();
            _fadeUI.Open();
            InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateVisuals);
            _actionIndicator.action.performed += KeyPressed;
        }

        private void UpdateVisuals(bool isGamepad) {
            _icon.sprite = isGamepad ? _indicatorGamepad : _indicatorKeyboard;
        }

        private void KeyPressed(InputAction.CallbackContext obj) {
            _fadeUI.Close();
            _actionIndicator.action.performed -= KeyPressed;
        }

    }
}