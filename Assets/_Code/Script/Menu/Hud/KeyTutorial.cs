using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.Player;

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

        private void OnDestroy() {
            if (_icon.enabled && Pause.Instance && InputCallbacks.Instance) {
                _actionIndicator.action.performed -= KeyPressed;
                Pause.Instance.onPause.RemoveListener(IconDisable);
                Pause.Instance.onUnpause.RemoveListener(IconEnable);
                InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisuals);
            }
        }

        public void StartTutorial() {
            _fadeUI.Open();
            _actionIndicator.action.performed += KeyPressed;
            Pause.Instance.onPause.AddListener(IconDisable);
            Pause.Instance.onUnpause.AddListener(IconEnable);
            InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateVisuals);
        }

        private void KeyPressed(InputAction.CallbackContext obj) {
            _fadeUI.Close();
            _actionIndicator.action.performed -= KeyPressed;
            Pause.Instance.onPause.RemoveListener(IconDisable);
            Pause.Instance.onUnpause.RemoveListener(IconEnable);
            InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisuals);
        }

        private void UpdateVisuals(bool isGamepad) {
            _icon.sprite = isGamepad ? _indicatorGamepad : _indicatorKeyboard;
        }

        private void IconEnable() {
            _icon.enabled = true;
        }

        private void IconDisable() {
            _icon.enabled = false;
        }

    }
}