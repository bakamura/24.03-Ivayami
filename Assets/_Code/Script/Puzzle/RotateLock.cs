using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    public class RotateLock : PasswordUI
    {
        [SerializeField] private ButtonDetails[] _buttonDetails;
        [SerializeField] private InputActionReference _navegationUIInput;
        [SerializeField, Min(.02f)] private float _animationDuration;

        [Serializable]
        private struct ButtonDetails
        {
            public GameObject Model;
            public Color[] Colors;
            public sbyte CurrentIndex;
        }

        private sbyte _currentBtnSelectedIndex;
        private Coroutine _animateCoroutine;
        private const sbyte _maxSize = 10;

        public override void UpdateActiveState(bool isActive)
        {
            base.UpdateActiveState(isActive);
            if (isActive)
            {
                _navegationUIInput.action.performed += HandleNavigateUI;
            }
            else
            {
                _navegationUIInput.action.performed -= HandleNavigateUI;
            }
        }

        public override bool CheckPassword()
        {
            string currentPassword = null;
            for(int i = 0; i < _buttonDetails.Length; i++)
            {
                currentPassword += _buttonDetails[i].CurrentIndex;
            }
            return string.Equals(currentPassword, password);
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            _currentBtnSelectedIndex += (sbyte)context.ReadValue<Vector2>().x;
            if (_currentBtnSelectedIndex >= _maxSize) _currentBtnSelectedIndex = 0;
            else if (_currentBtnSelectedIndex < 0) _currentBtnSelectedIndex = _maxSize - 1;
            if(context.ReadValue<Vector2>().y != 0)
            {

            }
        }

        private IEnumerator AnimateButtonCoroutine()
        {
            float count = 0;
            while(count < 1)
            {
                yield return null;
            }
        }

        private void OnValidate()
        {
            for(int i = 0; i < _buttonDetails.Length; i++)
            {
                if(_buttonDetails[i].Colors == null)
                {
                    _buttonDetails[i].Colors = new Color[10];                    
                } 
                else if(_buttonDetails[i].Colors.Length != _maxSize)
                {
                    //Array.Resize(_buttonDetails[i].Colors, );
                }
            }
        }
    }
}