using System;
using System.Collections;
using System.Collections.Generic;
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

        private sbyte _currentIndex;
        private TMP_Text[] _texts;
        private TMP_Text _currentHidden;

        [Serializable]
        private struct ButtonDetails
        {
            public Color[] Colors;
            public string[] Content;
        }        

        private void Awake()
        {
            //will always be 2
            _texts = GetComponentsInChildren<TMP_Text>();
            InteratctableHighlight = GetComponent<InteratctableHighlight>();
            _currentHidden = _texts[1];
            for(int i = 0; i < _texts.Length; i++)
            {
                _texts[i].text = _buttonDetails.Content[i];
                _texts[i].color = _buttonDetails.Colors[i];
            }
        }

        public void UpdateButtonDisplay(sbyte direction, float rotationAngle)
        {
            _currentHidden.transform.localPosition *= direction * Mathf.Sin(rotationAngle);
            _currentIndex += direction;
            if (_currentIndex >= _optionsAmount) _currentIndex = 0;
            else if (_currentIndex < 0) _currentIndex = (sbyte)(_optionsAmount - 1);
            _currentHidden.text = _buttonDetails.Content[_currentIndex];
            _currentHidden.color = _buttonDetails.Colors[_currentIndex];
            _currentHidden = _currentHidden.GetInstanceID() == _texts[0].GetInstanceID() ? _texts[1] : _texts[0];
        }

        public string GetCurrentDisplayValue()
        {
            return _buttonDetails.Content[_currentIndex];
        }
        private void OnValidate()
        {            
            if(_buttonDetails.Content != null && _buttonDetails.Content.Length != _optionsAmount)
            {
                Array.Resize(ref _buttonDetails.Content, _optionsAmount);
            }
            if(_buttonDetails.Colors != null && _buttonDetails.Colors.Length != _optionsAmount)
            {
                Array.Resize(ref _buttonDetails.Colors, _optionsAmount);
            }
        }
    }
}