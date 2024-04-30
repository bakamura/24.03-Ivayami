using System;
using UnityEngine;
using TMPro;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteratctableHighlight))]
    public class RotateLockButton : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _optionsAmount;
        [SerializeField] private ButtonDetails _buttonDetails;
        public InteratctableHighlight InteratctableHighlight { get; private set; }

        private sbyte _currentDetailIndex;
        //will always be 3
        private TMP_Text[] _texts;
        private sbyte _currentTextIndex;
        private float _modelRadius;

        [Serializable]
        private struct ButtonDetails
        {
            public Color[] Colors;
            public string[] Content;
        }

        private void Awake()
        {            
            _texts = GetComponentsInChildren<TMP_Text>();
            InteratctableHighlight = GetComponent<InteratctableHighlight>();
            _modelRadius = GetComponent<MeshFilter>().mesh.bounds.size.z / 2;

            _texts[1].text = _buttonDetails.Content[0];
            _texts[1].color = _buttonDetails.Colors[0];
        }

        public void UpdateButtonDisplay(sbyte direction)
        {
            if (direction != 0)
            {
                _currentDetailIndex += direction;
                if (_currentDetailIndex >= _optionsAmount) _currentDetailIndex = 0;
                else if (_currentDetailIndex < 0) _currentDetailIndex = (sbyte)(_optionsAmount - 1);
                _currentTextIndex = (sbyte)(direction > 0 ? 0 : 2);
                _texts[_currentTextIndex].text = _buttonDetails.Content[_currentDetailIndex];
                _texts[_currentTextIndex].color = _buttonDetails.Colors[_currentDetailIndex];
            }
            else
            {
                _texts[1].text = _texts[_currentTextIndex].text;
                _texts[1].color = _texts[_currentTextIndex].color;
            }
        }

        public string GetCurrentDisplayValue()
        {
            return _buttonDetails.Content[_currentDetailIndex];
        }

        private void OnValidate()
        {
            if (_buttonDetails.Content != null && _buttonDetails.Content.Length != _optionsAmount)
            {
                Array.Resize(ref _buttonDetails.Content, _optionsAmount);
            }
            if (_buttonDetails.Colors != null && _buttonDetails.Colors.Length != _optionsAmount)
            {
                Array.Resize(ref _buttonDetails.Colors, _optionsAmount);
            }
        }
    }
}