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

        private Image _icon;
        private Fade _fadeUI;

        private void Awake() {
            _icon = GetComponentInChildren<Image>();
            _fadeUI = GetComponentInChildren<Fade>();
        }

        private void Start() {
            Pause.Instance.onPause.AddListener(() => _icon.enabled = false);
            Pause.Instance.onPause.AddListener(() => _icon.enabled = true);
        }

        public void StartTutorial() {
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