using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Ivayami.Puzzle
{
    public class ReorderPassword : PasswordUI
    {
        [SerializeField] private RectTransform _buttonsContainer;
        [SerializeField] private Color _selectedButtonColor = Color.red;

        private Button _currentChosenBtn;
        private Button _currentSelectedBtn;

        public override bool CheckPassword()
        {
            string temp = "";
            foreach(TMP_Text text in _buttonsContainer.GetComponentsInChildren<TMP_Text>())
            {
                temp += text.text;
            }
            return string.Equals(temp, password);
        }

        public void UpdateSelectedBtn()
        {
            //Button btn = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            //Image btnImage = btn.GetComponent<Image>();
            //_lock.LockSounds.PlaySound(Audio.LockPuzzleSounds.SoundTypes.ConfirmOption);
            //selected same button as active
            if (_currentSelectedBtn == _currentChosenBtn)
            {
                _currentSelectedBtn.GetComponent<Image>().color = Color.white;
                _currentChosenBtn = null;
            }
            //selected button to start the place change
            else if (_currentChosenBtn == null)
            {
                _currentSelectedBtn.GetComponent<Image>().color = _selectedButtonColor;
                _currentChosenBtn = _currentSelectedBtn;
            }
            //change button places
            else if(_currentSelectedBtn != _currentChosenBtn && _currentChosenBtn != null)
            {
                _currentChosenBtn.GetComponent<Image>().color = Color.white;
                _currentSelectedBtn.GetComponent<Image>().color = Color.white;
                int currentChosenIndex = _currentChosenBtn.transform.GetSiblingIndex();
                _currentChosenBtn.transform.SetSiblingIndex(_currentSelectedBtn.transform.GetSiblingIndex());
                _currentSelectedBtn.transform.SetSiblingIndex(currentChosenIndex);
                _currentChosenBtn = null;
                OnCheckPassword?.Invoke();
            }
        }

        public void SetCurrentSelected(Button btn)
        {
            _currentSelectedBtn = btn;
        }
    }
}