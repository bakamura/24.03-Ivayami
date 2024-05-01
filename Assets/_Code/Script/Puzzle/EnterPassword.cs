using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using TMPro;

namespace Ivayami.Puzzle
{
    public class EnterPassword : PasswordUI
    {
        [SerializeField] private TMP_InputField _passwordTextField;        
        private const string _incorrectPasswordText = "INCORRECT";
        public override bool CheckPassword()
        {
            bool result = string.Equals(_passwordTextField.text, password);
            if (!result) _passwordTextField.text = _incorrectPasswordText;
            return result;
        }

        public override void UpdateActiveState(bool isActive)
        {
            base.UpdateActiveState(isActive);
            if (isActive) _passwordTextField.text = "";
        }

        public void InsertCharacter(TMP_Text text)
        {
            if (_passwordTextField.text == _incorrectPasswordText) _passwordTextField.text = "";
            if (_passwordTextField.text.Length < _passwordTextField.characterLimit) _passwordTextField.text += text.text;
            if(_passwordTextField.text.Length == _passwordTextField.characterLimit) _onCheckPassword?.Invoke();
        }

        private void OnValidate()
        {
            if (_passwordTextField)
            {
                _passwordTextField.characterLimit = password.Length;
            }
        }
    }
}