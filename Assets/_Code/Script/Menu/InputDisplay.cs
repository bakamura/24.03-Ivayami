using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class InputDisplay : MonoSingleton<InputDisplay> {

        [Header("References")]

        [SerializeField] private RectTransform _resizeContainer;
        [SerializeField] private Transform _diplaysContainer;
        [SerializeField] private RectTransform _displayPrefab;

        private List<Display> _displays = new List<Display>();

        [Header("Cache")]

        private Fade _fade;
        private float _heightBase;
        private InputIcons[] _inputIconsCurrent;
        private int _controlType;

        [Serializable]
        private struct Display {
            [SerializeField] private Image _image;
            [SerializeField] private LocalizeStringEvent _localizedString;
            public Display(Image image, LocalizeStringEvent localizedString) {
                _image = image;
                _localizedString = localizedString;
            }

            public bool activeSelf => _image.transform.parent.gameObject.activeSelf;
            public void SetActive(bool isActive) => _image.transform.parent.gameObject.SetActive(isActive);
            public void DisplayInput(InputIcons icons, int controlType) {
                _image.sprite = icons.Icons[controlType];
                _localizedString.SetEntry(icons.InputName);
                SetActive(true);
            }
        }

        private void Start() {
            TryGetComponent(out _fade);
            _heightBase = _resizeContainer.sizeDelta.y;
            InputCallbacks.Instance.SubscribeToOnChangeControls(OnChangeControls);
        }

        public void DisplayInputs(InputIcons[] icons) {
            _inputIconsCurrent = icons;
            for (int i = 0; i < _inputIconsCurrent.Length; i++) {
                if (_displays.Count <= i) {
                    GameObject newInstance = Instantiate(_displayPrefab.gameObject, _diplaysContainer);
                    _displays.Add(new Display(newInstance.GetComponentInChildren<Image>(), newInstance.GetComponentInChildren<LocalizeStringEvent>()));
                }
                _displays[i].DisplayInput(_inputIconsCurrent[i], _controlType);
            }
            for (int i = _inputIconsCurrent.Length; i < _displays.Count; i++) _displays[i].SetActive(false);
            _resizeContainer.sizeDelta = new Vector2(_resizeContainer.sizeDelta.x, _heightBase + (_inputIconsCurrent.Length * _displayPrefab.sizeDelta.y));

            _fade.Open();
        }

        public void Hide() {
            _fade.Close();
        }

        private void OnChangeControls(InputCallbacks.ControlType controlType) {
            _controlType = (int)controlType;
            for (int i = 0; i < _displays.Count; i++) if (_displays[i].activeSelf) _displays[i].DisplayInput(_inputIconsCurrent[i], _controlType);
        }

    }
}
