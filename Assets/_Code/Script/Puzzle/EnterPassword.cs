using UnityEngine;
using TMPro;

namespace Ivayami.Puzzle
{
    public class EnterPassword : PasswordUI
    {
        [SerializeField] private TMP_Text _passwordTextField;
        private const string _incorrectPasswordText = "INCORRECT";
        private const char _emptyCharacter = '-';
        private string _currentPassword;
        public override bool CheckPassword()
        {
            bool result = string.Equals(_passwordTextField.text, password);
            if (!result) _passwordTextField.text = _incorrectPasswordText;
            return result;
        }

        public override void UpdateActiveState(bool isActive)
        {
            base.UpdateActiveState(isActive);
            if (isActive) EraseAll();
        }

        public void InsertCharacter(TMP_Text text)
        {
            if (_currentPassword.Length >= password.Length && _passwordTextField.text != _incorrectPasswordText) return;
            if (_passwordTextField.text == _incorrectPasswordText) EraseAll();
            _currentPassword += text.text;
            FormatPasswordText();
            _lock.LockSounds.PlaySound(Audio.LockPuzzleSounds.SoundTypes.ConfirmOption);
            //if(_passwordTextField.text.Length == _passwordTextField.characterLimit) OnCheckPassword?.Invoke();
        }

        private void FormatPasswordText()
        {
            _passwordTextField.text = _currentPassword;
            for (int i = _currentPassword.Length; i < password.Length; i++)
            {
                _passwordTextField.text += _emptyCharacter;
            }
        }

        public void RemoveCharacter()
        {
            _currentPassword = _currentPassword.Remove(_currentPassword.Length - 1);
            FormatPasswordText();
        }

        public void EraseAll()
        {
            _currentPassword = "";
            FormatPasswordText();
        }
    }
}