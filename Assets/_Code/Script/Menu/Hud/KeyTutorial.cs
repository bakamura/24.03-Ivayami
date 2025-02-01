using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    [RequireComponent(typeof(Fade))]
    public class KeyTutorial : MonoBehaviour {

        [SerializeField] private InputActionReference _actionIndicator;
        [SerializeField] private InputIcons _indicators;

        private Image _icon;
        private Fade _fadeUI;

        private void Awake() {
            _icon = GetComponentInChildren<Image>();
            _fadeUI = GetComponentInChildren<Fade>();
        }

        private void OnDestroy() {
            if (_icon.enabled) {
                _actionIndicator.action.performed -= KeyPressed;
                Pause.Instance?.onPause.RemoveListener(IconDisable);
                Pause.Instance?.onUnpause.RemoveListener(IconEnable);
                InputCallbacks.Instance?.UnsubscribeToOnChangeControls(UpdateVisuals);
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

        private void UpdateVisuals(InputCallbacks.ControlType controlType) {
            _icon.sprite = _indicators.Icons[(int)controlType];
        }

        private void IconEnable() {
            _icon.enabled = true;
        }

        private void IconDisable() {
            _icon.enabled = false;
        }

    }
}