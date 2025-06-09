using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class ButtonSwapperGamepad : MonoBehaviour {

        [SerializeField] private Button _substitutedBtn;
        [SerializeField] private InputIcons _inputIcons;
        private Image _representGamepadBtn;

        private void Awake() {
            _representGamepadBtn = GetComponent<Image>();
        }

        private void Start() {
            InputCallbacks.Instance.SubscribeToOnChangeControls(SwapBtn);
        }

        private void SwapBtn(InputCallbacks.ControlType controlType) {
            bool isGamepad = controlType != InputCallbacks.ControlType.Keyboard;
            _substitutedBtn?.gameObject.SetActive(!isGamepad);
            _representGamepadBtn.gameObject.SetActive(isGamepad);
            if (_representGamepadBtn.gameObject.activeSelf) _representGamepadBtn.sprite = _inputIcons.Icons[(int)controlType];
        }

    }
}
