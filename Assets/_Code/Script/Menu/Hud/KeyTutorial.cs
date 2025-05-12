using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.Player;
using UnityEngine.Localization.Components;
using TMPro;

namespace Ivayami.UI {
    [RequireComponent(typeof(Fade))]
    public class KeyTutorial : MonoBehaviour {

        [SerializeField] private InputActionReference _actionIndicator;
        [SerializeField] private InputIcons _indicators;

        [Space(24)]

        [SerializeField] private Image _icon;
        [SerializeField] private LocalizeStringEvent _text;
        private TextMeshProUGUI _textBox;
        [SerializeField] private RectTransform _container;
        
        private Fade _fadeUI;
        private float _containerBaseWidth;

        private void Awake() {
            _fadeUI = GetComponentInChildren<Fade>();
            _textBox = _text.GetComponent<TextMeshProUGUI>();
            _containerBaseWidth = _container.sizeDelta.x;
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
            if (!gameObject.activeInHierarchy) return;
            _fadeUI.Open();
            _actionIndicator.action.performed += KeyPressed;
            Pause.Instance.onPause.AddListener(IconDisable);
            Pause.Instance.onUnpause.AddListener(IconEnable);
            _text.SetEntry(_indicators.InputName);
            _container.sizeDelta = new Vector2(_containerBaseWidth + _textBox.preferredWidth, _container.sizeDelta.y);
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